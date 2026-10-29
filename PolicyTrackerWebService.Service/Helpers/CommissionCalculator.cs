using PolicyTrackerWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Service.Helpers
{
    /// <summary>
    /// Expected points / commission and statement status for policies.
    /// </summary>
    public class CommissionCalculator
    {
        private const int OverdueDays = 45;

        private readonly Dictionary<int, Domain.Category> _categories;
        private readonly List<Domain.PointsGrid> _grid;
        private readonly ILookup<int, Domain.StatementLine> _linesByPolicy;
        private readonly Dictionary<int, HashSet<DateTime>> _loadedMonths;
        private readonly Dictionary<int, DateTime> _trackingStart;
        private readonly DateTime _today;

        public CommissionCalculator(IEnumerable<Domain.Category> categories, IEnumerable<Domain.PointsGrid> grid, IEnumerable<Domain.StatementLine> lines, DateTime today)
        {
            _categories = categories.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
            _grid = grid.Where(g => g.IsActive).ToList();

            var lineList = lines.ToList();
            _linesByPolicy = lineList.Where(l => l.PolicyId.HasValue).ToLookup(l => l.PolicyId.Value);
            _loadedMonths = lineList.GroupBy(l => l.BrokerId)
                                    .ToDictionary(g => g.Key, g => new HashSet<DateTime>(g.Select(l => l.StatementMonth.FirstOfMonth())));
            _trackingStart = _loadedMonths.ToDictionary(kv => kv.Key, kv => kv.Value.Min());
            _today = today.Date;
        }

        public void Apply(Domain.Policy p)
        {
            Domain.Category category = null;
            if (p.CategoryId.HasValue) _categories.TryGetValue(p.CategoryId.Value, out category);
            if (category != null)
            {
                p.IsMotor = category.IsMotor;
                if (p.Category.IsNullOrEmpty()) p.Category = category.Name;
            }

            // ---- expected points & commission ----
            p.ExpectedPoints = null;
            p.ExpectedCommission = null;

            if (p.PointsOd.HasValue || p.PointsTp.HasValue)
            {
                p.ExpectedCommission = ((p.OdPremium ?? 0) * (p.PointsOd ?? 0) / 100m + (p.TpPremium ?? 0) * (p.PointsTp ?? 0) / 100m).RoundMoney();
            }
            else
            {
                p.ExpectedPoints = p.PointsNet ?? GridPoints(p) ?? (category != null ? category.DefaultPoints : null);
                if (p.ExpectedPoints.HasValue && p.NetPremium.HasValue)
                    p.ExpectedCommission = (p.NetPremium.Value * p.ExpectedPoints.Value / 100m).RoundMoney();
            }

            // ---- days to expiry ----
            p.DaysToExpiry = p.ExpiryDate.HasValue ? (int?)(p.ExpiryDate.Value.Date - _today).TotalDays : null;

            // ---- statement status ----
            var lines = _linesByPolicy[p.Id].ToList();
            p.CommissionReceived = null;
            p.PointsPaid = null;
            p.Shortfall = null;
            p.StatementMonth = null;

            if (lines.Count > 0)
            {
                p.CommissionReceived = lines.Sum(l => l.Value);
                var first = lines.OrderBy(l => l.StatementMonth).First();
                p.PointsPaid = first.PointsNet ?? first.PointsOd ?? first.PointsTp;
                p.StatementMonth = first.StatementMonth;
                p.StatementStatus = lines.Any(l => l.IsOtherPosCode)
                    ? Enums.StatementStatus.Other_POS_Code.ToText()
                    : Enums.StatementStatus.Found.ToText();
            }
            else
            {
                p.StatementStatus = PendingStatus(p);
            }

            if (p.ExpectedCommission.HasValue &&
                (p.StatementStatus.Is(Enums.StatementStatus.Found) || p.StatementStatus.Is(Enums.StatementStatus.Missing) ||
                 p.StatementStatus.Is(Enums.StatementStatus.Other_POS_Code) || p.StatementStatus.Is(Enums.StatementStatus.Overdue)))
            {
                p.Shortfall = p.ExpectedCommission.Value - (p.StatementStatus.Is(Enums.StatementStatus.Found) ? (p.CommissionReceived ?? 0) : 0);
            }
        }

        private string PendingStatus(Domain.Policy p)
        {
            var notApplicable = Enums.StatementStatus.Not_Applicable.ToText();

            // not a placed policy term (e.g. last year's term kept only for its renewal)
            if (!p.StartDate.HasValue) return notApplicable;
            if (p.RenewalStatus.Is(Enums.RenewalStatus.Lost) && p.RenewedPolicyId == null && p.PolicyNo.IsNullOrEmpty()) return notApplicable;

            DateTime trackingStart;
            if (!_trackingStart.TryGetValue(p.BrokerId, out trackingStart) || p.StartDate.Value.FirstOfMonth() < trackingStart)
                return notApplicable;

            HashSet<DateTime> loaded;
            _loadedMonths.TryGetValue(p.BrokerId, out loaded);
            loaded = loaded ?? new HashSet<DateTime>();

            var startMonth = p.StartDate.Value.FirstOfMonth();
            var nextMonth = startMonth.AddMonths(1);

            // Policies starting at month-end sometimes appear in the next month's statement.
            if (loaded.Contains(startMonth) && (p.StartDate.Value.Day <= 25 || loaded.Contains(nextMonth)))
                return Enums.StatementStatus.Missing.ToText();

            if ((_today - p.StartDate.Value.Date).TotalDays > OverdueDays)
                return Enums.StatementStatus.Overdue.ToText();

            return Enums.StatementStatus.Awaiting_Statement.ToText();
        }

        private decimal? GridPoints(Domain.Policy p)
        {
            if (!p.CategoryId.HasValue || !p.EldestAge.HasValue) return null;

            var row = _grid.Where(g => g.BrokerId == p.BrokerId && g.CategoryId == p.CategoryId.Value &&
                                       p.EldestAge.Value >= g.AgeFrom && p.EldestAge.Value <= g.AgeTo)
                           .OrderBy(g => g.AgeTo - g.AgeFrom)
                           .FirstOrDefault();

            return row == null ? (decimal?)null : row.Points;
        }

        /// <summary>Commission the broker should have paid on a statement line (premium × points).</summary>
        public static decimal? LineValue(Domain.StatementLine line)
        {
            if (line.PointsOd.HasValue || line.PointsTp.HasValue)
                return ((line.OdPremium ?? 0) * (line.PointsOd ?? 0) / 100m + (line.TpPremium ?? 0) * (line.PointsTp ?? 0) / 100m).RoundMoney();

            if (line.PointsNet.HasValue && line.NetPremium.HasValue)
                return (line.NetPremium.Value * line.PointsNet.Value / 100m).RoundMoney();

            return null;
        }
    }
}

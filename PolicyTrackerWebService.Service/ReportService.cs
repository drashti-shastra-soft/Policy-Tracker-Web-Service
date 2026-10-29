using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Service
{
    public class ReportService : IReportService
    {
        #region Declaration
        private readonly IPolicyService _policyService;
        private readonly IPolicyRepository _policyRepo;
        private readonly IStatementRepository _statementRepo;
        private readonly IPayoutRepository _payoutRepo;
        private readonly IBrokerRepository _brokerRepo;

        public ReportService(IPolicyService policyService, IPolicyRepository policyRepo, IStatementRepository statementRepo,
                             IPayoutRepository payoutRepo, IBrokerRepository brokerRepo)
        {
            _policyService = policyService;
            _policyRepo = policyRepo;
            _statementRepo = statementRepo;
            _payoutRepo = payoutRepo;
            _brokerRepo = brokerRepo;
        }
        #endregion

        #region Public Methods
        public Domain.DashboardSummary GetDashboard(DateTime month, int? brokerId)
        {
            var summary = new Domain.DashboardSummary { Month = month.FirstOfMonth(), BrokerId = brokerId };

            var expiring = _policyService.GetDueList(summary.Month, brokerId, true);
            summary.DueCount = expiring.Count;
            summary.RenewedCount = expiring.Count(x => x.RenewalStatus.Is(Enums.RenewalStatus.Renewed));
            summary.LostCount = expiring.Count(x => x.RenewalStatus.Is(Enums.RenewalStatus.Lost));
            summary.DueList = expiring.Where(x => !x.RenewalStatus.Is(Enums.RenewalStatus.Renewed) && !x.RenewalStatus.Is(Enums.RenewalStatus.Lost)).ToList();
            summary.PendingCount = summary.DueList.Count;
            summary.PremiumAtRisk = summary.DueList.Sum(x => x.IndicativePremium ?? x.NetPremium ?? 0m);

            var report = GetReconciliation(brokerId);
            summary.NotOnStatement = report.MissingFromStatement;
            summary.MissingFromStatementCount = report.MissingFromStatement.Count;
            summary.CommissionPerStatement = report.TotalCommission;
            summary.ReceivedInBank = report.TotalReceived;
            summary.UnexplainedGap = report.TotalUnexplained;
            return summary;
        }

        public Domain.ReconciliationReport GetReconciliation(int? brokerId)
        {
            var report = new Domain.ReconciliationReport { BrokerId = brokerId, GeneratedOn = DateTime.Now };
            var brokers = _brokerRepo.GetActive();
            if (brokerId.HasValue && brokerId > 0)
                report.Broker = brokers.Where(b => b.Id == brokerId).Select(b => b.Name).FirstOrDefault();

            var lines = _statementRepo.GetByBroker(brokerId);
            var payouts = _payoutRepo.Search(new Domain.Payout { BrokerId = brokerId ?? 0 });
            var rates = brokers.ToDictionary(b => b.Id, b => b.TdsRate);

            // ---- month-wise statement vs bank ----
            foreach (var g in lines.GroupBy(l => new { l.BrokerId, Month = l.StatementMonth.FirstOfMonth() }).OrderBy(g => g.Key.Month))
            {
                var monthPayouts = payouts.Where(p => p.BrokerId == g.Key.BrokerId && p.StatementMonth.HasValue && p.StatementMonth.Value.FirstOfMonth() == g.Key.Month).ToList();
                var rate = monthPayouts.Where(p => p.TdsRate.HasValue).Select(p => p.TdsRate.Value).FirstOrDefault();
                if (rate == 0m && rates.ContainsKey(g.Key.BrokerId)) rate = rates[g.Key.BrokerId];

                var m = new Domain.MonthReconciliation
                {
                    StatementMonth = g.Key.Month,
                    Policies = g.Count(),
                    Commission = g.Sum(l => l.Value),
                    Received = monthPayouts.Sum(p => p.AmountReceived),
                    ReceivedOn = monthPayouts.Max(p => p.ReceivedOn),
                    TdsRate = rate,
                };
                m.Tds = (m.Commission * rate / 100m).RoundMoney();
                m.ShortBy = m.Commission - m.Received;
                m.Unexplained = m.ShortBy - m.Tds;
                m.PercentReceived = m.Received > 0 && m.Commission > 0 ? Math.Round(m.Received / m.Commission * 100m, 1) : (decimal?)null;
                report.Months.Add(m);
            }

            report.UnallocatedPayouts = payouts.Where(p => !p.StatementMonth.HasValue).ToList();

            report.TotalCommission = report.Months.Sum(m => m.Commission);
            report.TotalReceived = report.Months.Sum(m => m.Received) + report.UnallocatedPayouts.Sum(p => p.AmountReceived);
            report.TotalTds = report.Months.Sum(m => m.Tds);
            report.TotalShortBy = report.TotalCommission - report.TotalReceived;
            report.TotalUnexplained = report.TotalShortBy - report.TotalTds;

            // ---- renewals we placed that are not on the statement ----
            var policies = _policyService.Calculate(_policyRepo.Search(new Domain.Policy { BrokerId = brokerId ?? 0 }));
            report.MissingFromStatement = policies.Where(p => p.StatementStatus.Is(Enums.StatementStatus.Missing) || p.StatementStatus.Is(Enums.StatementStatus.Overdue))
                                                  .OrderBy(p => p.StartDate).ToList();
            report.MissingPremium = report.MissingFromStatement.Sum(p => p.NetPremium ?? p.IndicativePremium ?? 0m);
            report.MissingCommissionEstimate = report.MissingFromStatement.Sum(p => p.ExpectedCommission ?? 0m);

            // ---- points / arithmetic issues ----
            report.PointsIssues = FindPointsIssues(lines, policies);

            // ---- data issues in the broker file ----
            report.DataIssues = lines.Where(l => l.PolicyNo.IsNullOrEmpty() || l.PolicyNo.Contains("E+") || l.IsOtherPosCode)
                                     .OrderBy(l => l.StatementMonth).ToList();
            return report;
        }
        #endregion

        #region Private Methods
        private static List<Domain.PointsIssue> FindPointsIssues(List<Domain.StatementLine> lines, List<Domain.Policy> policies)
        {
            var issues = new Dictionary<int, Domain.PointsIssue>();
            var byId = policies.ToDictionary(p => p.Id);

            Action<Domain.StatementLine, decimal?, decimal?, string> add = (line, expectedPoints, expectedValue, text) =>
            {
                Domain.PointsIssue issue;
                if (!issues.TryGetValue(line.Id, out issue))
                {
                    issue = new Domain.PointsIssue
                    {
                        StatementLineId = line.Id,
                        PolicyId = line.PolicyId,
                        CustomerName = line.CustomerName,
                        PolicyNo = line.PolicyNo,
                        Product = line.Product,
                        NetPremium = line.NetPremium,
                        PointsPaid = line.PointsNet ?? line.PointsOd ?? line.PointsTp,
                        Value = line.Value,
                    };
                    issues[line.Id] = issue;
                }

                if (expectedPoints.HasValue) issue.PointsExpected = expectedPoints;
                if (expectedValue.HasValue)
                {
                    issue.ValueExpected = expectedValue;
                    issue.Difference = expectedValue - line.Value;
                }
                issue.Issue = issue.Issue.IsNullOrEmpty() ? text : issue.Issue + " | " + text;
            };

            foreach (var line in lines)
            {
                // 1. broker's own arithmetic
                var calc = CommissionCalculator.LineValue(line);
                if (calc.HasValue && Math.Abs(calc.Value - line.Value) > 1)
                    add(line, null, calc, string.Format("Value ₹{0:N0} does not equal premium × points (₹{1:N0})", line.Value, calc.Value));

                // 2. points different from what we expect for this policy
                Domain.Policy policy;
                var paid = line.PointsNet;
                if (line.PolicyId.HasValue && byId.TryGetValue(line.PolicyId.Value, out policy) && policy.PointsNet.HasValue && paid.HasValue && policy.PointsNet.Value != paid.Value)
                {
                    var expected = line.NetPremium.HasValue ? (line.NetPremium.Value * policy.PointsNet.Value / 100m).RoundMoney() : (decimal?)null;
                    add(line, policy.PointsNet, expected, string.Format("Paid {0:0.##} points, expected {1:0.##}", paid, policy.PointsNet));
                }
            }

            // 3. same product + same premium but fewer points than the others
            var peers = lines.Where(l => l.NetPremium.HasValue && (l.PointsNet ?? l.PointsOd).HasValue)
                             .GroupBy(l => new { Product = (l.Product ?? string.Empty).ToUpperInvariant(), Premium = l.NetPremium.Value })
                             .Where(g => g.Count() >= 2);

            foreach (var g in peers)
            {
                var max = g.Max(l => (l.PointsNet ?? l.PointsOd).Value);
                foreach (var line in g.Where(l => (l.PointsNet ?? l.PointsOd).Value < max))
                {
                    var expected = (line.NetPremium.Value * max / 100m).RoundMoney();
                    add(line, max, expected, string.Format("Paid {0:0.##} points; other {1} policies with the same premium ₹{2:N0} got {3:0.##}",
                        (line.PointsNet ?? line.PointsOd).Value, g.Key.Product.ToLowerInvariant(), g.Key.Premium, max));
                }
            }

            return issues.Values.OrderByDescending(i => i.Difference ?? 0).ToList();
        }
        #endregion
    }
}

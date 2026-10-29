using System;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class ReconciliationReport
    {
        public int? BrokerId { get; set; }
        public string Broker { get; set; }
        public DateTime GeneratedOn { get; set; }

        public List<MonthReconciliation> Months { get; set; } = new List<MonthReconciliation>();
        public List<Payout> UnallocatedPayouts { get; set; } = new List<Payout>();
        public List<Policy> MissingFromStatement { get; set; } = new List<Policy>();
        public List<PointsIssue> PointsIssues { get; set; } = new List<PointsIssue>();
        public List<StatementLine> DataIssues { get; set; } = new List<StatementLine>();

        public decimal TotalCommission { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalShortBy { get; set; }
        public decimal TotalTds { get; set; }
        public decimal TotalUnexplained { get; set; }
        public decimal MissingPremium { get; set; }
        public decimal MissingCommissionEstimate { get; set; }
    }

    public class MonthReconciliation
    {
        public DateTime StatementMonth { get; set; }
        public int Policies { get; set; }
        public decimal Commission { get; set; }
        public decimal Received { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public decimal ShortBy { get; set; }
        public decimal TdsRate { get; set; }
        public decimal Tds { get; set; }
        public decimal Unexplained { get; set; }
        public decimal? PercentReceived { get; set; }
    }

    public class PointsIssue
    {
        public int StatementLineId { get; set; }
        public int? PolicyId { get; set; }
        public string CustomerName { get; set; }
        public string PolicyNo { get; set; }
        public string Product { get; set; }
        public decimal? NetPremium { get; set; }
        public decimal? PointsPaid { get; set; }
        public decimal? PointsExpected { get; set; }
        public decimal Value { get; set; }
        public decimal? ValueExpected { get; set; }
        public decimal? Difference { get; set; }
        public string Issue { get; set; }
    }
}

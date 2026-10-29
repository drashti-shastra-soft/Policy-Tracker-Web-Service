using System;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class DashboardSummary
    {
        public DateTime Month { get; set; }
        public int? BrokerId { get; set; }

        public int DueCount { get; set; }
        public int RenewedCount { get; set; }
        public int PendingCount { get; set; }
        public int LostCount { get; set; }
        public decimal PremiumAtRisk { get; set; }

        public int MissingFromStatementCount { get; set; }
        public decimal CommissionPerStatement { get; set; }
        public decimal ReceivedInBank { get; set; }
        public decimal UnexplainedGap { get; set; }

        /// <summary>Policies expiring in the month that are not renewed / lost yet.</summary>
        public List<Policy> DueList { get; set; } = new List<Policy>();

        /// <summary>Placed policies that have not appeared on any broker statement.</summary>
        public List<Policy> NotOnStatement { get; set; } = new List<Policy>();
    }
}

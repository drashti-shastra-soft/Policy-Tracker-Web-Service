using System;

namespace PolicyTrackerWebService.Domain
{
    /// <summary>One policy term placed (or due for renewal) through the broker.</summary>
    public class Policy
    {
        public int Id { get; set; }
        public int BrokerId { get; set; }
        public string Broker { get; set; }
        public string BusinessType { get; set; }
        public int? CategoryId { get; set; }
        public string Category { get; set; }
        public bool IsMotor { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public string InsuranceCompany { get; set; }
        public string ProductPlan { get; set; }

        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public string EmailAddress { get; set; }
        public int? EldestAge { get; set; }
        public decimal? SumInsured { get; set; }

        public string PolicyNo { get; set; }
        public string PreviousPolicyNo { get; set; }
        public string RenewalRefNo { get; set; }
        public string VehicleNo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public decimal? OdPremium { get; set; }
        public decimal? TpPremium { get; set; }
        public decimal? NetPremium { get; set; }
        public decimal? GstAmount { get; set; }
        public decimal? IndicativePremium { get; set; }

        /// <summary>Expected points as per broker grid; leave blank to use the grid / category default.</summary>
        public decimal? PointsOd { get; set; }
        public decimal? PointsTp { get; set; }
        public decimal? PointsNet { get; set; }

        // ---- Renewal follow-up (options, not handwriting) ----
        public string RenewalStatus { get; set; }
        public string LostReason { get; set; }
        public DateTime? QuoteSentOn { get; set; }
        public string QuoteDetails { get; set; }
        public DateTime? RenewedOn { get; set; }
        public int? RenewedPolicyId { get; set; }
        public int? RenewedFromPolicyId { get; set; }

        public string PaymentMode { get; set; }
        public string ChequeNo { get; set; }
        public string BankName { get; set; }
        public string UtrNo { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? SubmittedToBrokerOn { get; set; }
        public string BrokerStaff { get; set; }
        public string ProofRef { get; set; }
        public bool PolicyCopyReceived { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }

        // ---- Calculated (read only) ----
        public decimal? ExpectedPoints { get; set; }
        public decimal? ExpectedCommission { get; set; }
        public decimal? PointsPaid { get; set; }
        public decimal? CommissionReceived { get; set; }
        public decimal? Shortfall { get; set; }
        public string StatementStatus { get; set; }
        public DateTime? StatementMonth { get; set; }
        public int? DaysToExpiry { get; set; }

        // ---- Search only ----
        public string SearchText { get; set; }
        public DateTime? ExpiryFrom { get; set; }
        public DateTime? ExpiryTo { get; set; }
        public DateTime? StartFrom { get; set; }
        public DateTime? StartTo { get; set; }
        public string Status { get; set; }
    }
}

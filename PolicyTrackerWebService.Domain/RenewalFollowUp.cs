using System;

namespace PolicyTrackerWebService.Domain
{
    /// <summary>Outcome of a renewal, captured from dropdown options instead of handwritten notes.</summary>
    public class RenewalFollowUp
    {
        public int PolicyId { get; set; }
        public string RenewalStatus { get; set; }
        public string LostReason { get; set; }
        public DateTime? QuoteSentOn { get; set; }
        public string QuoteDetails { get; set; }

        public DateTime? RenewedOn { get; set; }
        public string NewPolicyNo { get; set; }
        public DateTime? NewStartDate { get; set; }
        public DateTime? NewExpiryDate { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public decimal? SumInsured { get; set; }
        public decimal? OdPremium { get; set; }
        public decimal? TpPremium { get; set; }
        public decimal? NetPremium { get; set; }
        public decimal? GstAmount { get; set; }

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

        public int LastModifiedBy { get; set; }

        /// <summary>Returned: id of the new policy term created when status is Renewed.</summary>
        public int? RenewedPolicyId { get; set; }
    }
}

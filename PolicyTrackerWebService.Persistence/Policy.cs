using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("Policies")]
    public class Policy : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int BrokerId { get; set; }

        [StringLength(30)]
        public string BusinessType { get; set; }

        public int? CategoryId { get; set; }

        public int? InsuranceCompanyId { get; set; }

        [StringLength(200)]
        public string ProductPlan { get; set; }

        [Required, StringLength(200)]
        public string CustomerName { get; set; }

        [StringLength(20)]
        public string MobileNo { get; set; }

        [StringLength(150)]
        public string EmailAddress { get; set; }

        public int? EldestAge { get; set; }

        public decimal? SumInsured { get; set; }

        [StringLength(100)]
        public string PolicyNo { get; set; }

        [StringLength(100)]
        public string PreviousPolicyNo { get; set; }

        [StringLength(100)]
        public string RenewalRefNo { get; set; }

        [StringLength(30)]
        public string VehicleNo { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public decimal? OdPremium { get; set; }

        public decimal? TpPremium { get; set; }

        public decimal? NetPremium { get; set; }

        public decimal? GstAmount { get; set; }

        public decimal? IndicativePremium { get; set; }

        public decimal? PointsOd { get; set; }

        public decimal? PointsTp { get; set; }

        public decimal? PointsNet { get; set; }

        [StringLength(30)]
        public string RenewalStatus { get; set; }

        [StringLength(50)]
        public string LostReason { get; set; }

        public DateTime? QuoteSentOn { get; set; }

        [StringLength(500)]
        public string QuoteDetails { get; set; }

        public DateTime? RenewedOn { get; set; }

        public int? RenewedPolicyId { get; set; }

        public int? RenewedFromPolicyId { get; set; }

        [StringLength(30)]
        public string PaymentMode { get; set; }

        [StringLength(50)]
        public string ChequeNo { get; set; }

        [StringLength(150)]
        public string BankName { get; set; }

        [StringLength(100)]
        public string UtrNo { get; set; }

        public decimal? AmountPaid { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime? SubmittedToBrokerOn { get; set; }

        [StringLength(100)]
        public string BrokerStaff { get; set; }

        [StringLength(300)]
        public string ProofRef { get; set; }

        public bool PolicyCopyReceived { get; set; }

        [StringLength(2000)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
    }
}

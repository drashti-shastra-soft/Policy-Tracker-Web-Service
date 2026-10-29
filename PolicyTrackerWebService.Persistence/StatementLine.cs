using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("StatementLines")]
    public class StatementLine
    {
        [Key]
        public int Id { get; set; }

        public int BrokerId { get; set; }

        public DateTime StatementMonth { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(150)]
        public string SalesManager { get; set; }

        [StringLength(150)]
        public string PosName { get; set; }

        [StringLength(50)]
        public string PosCode { get; set; }

        [StringLength(200)]
        public string CustomerName { get; set; }

        [StringLength(200)]
        public string InsuranceCompany { get; set; }

        [StringLength(200)]
        public string Product { get; set; }

        [StringLength(100)]
        public string PolicyNo { get; set; }

        public decimal? OdPremium { get; set; }

        public decimal? TpPremium { get; set; }

        public decimal? NetPremium { get; set; }

        public decimal? GstAmount { get; set; }

        public decimal? PointsOd { get; set; }

        public decimal? PointsTp { get; set; }

        public decimal? PointsNet { get; set; }

        public decimal Value { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public int? PolicyId { get; set; }

        [StringLength(30)]
        public string MatchStatus { get; set; }

        [StringLength(100)]
        public string ImportBatch { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? LastModifiedBy { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}

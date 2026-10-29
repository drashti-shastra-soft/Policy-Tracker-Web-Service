using System;

namespace PolicyTrackerWebService.Domain
{
    /// <summary>One row of the broker's commission statement.</summary>
    public class StatementLine
    {
        public int Id { get; set; }
        public int BrokerId { get; set; }
        public string Broker { get; set; }
        public DateTime StatementMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string SalesManager { get; set; }
        public string PosName { get; set; }
        public string PosCode { get; set; }
        public string CustomerName { get; set; }
        public string InsuranceCompany { get; set; }
        public string Product { get; set; }
        public string PolicyNo { get; set; }
        public decimal? OdPremium { get; set; }
        public decimal? TpPremium { get; set; }
        public decimal? NetPremium { get; set; }
        public decimal? GstAmount { get; set; }
        public decimal? PointsOd { get; set; }
        public decimal? PointsTp { get; set; }
        public decimal? PointsNet { get; set; }
        public decimal Value { get; set; }
        public string Remarks { get; set; }
        public int? PolicyId { get; set; }
        public string MatchStatus { get; set; }
        public string ImportBatch { get; set; }
        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }

        // calculated
        public decimal? CalculatedValue { get; set; }
        public decimal? ValueDifference { get; set; }
        public bool IsOtherPosCode { get; set; }
        public string LinkedCustomer { get; set; }

        // search only
        public string SearchText { get; set; }
    }
}

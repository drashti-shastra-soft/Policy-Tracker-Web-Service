using System;

namespace PolicyTrackerWebService.Domain
{
    /// <summary>Money actually received in the bank against a statement month.</summary>
    public class Payout
    {
        public int Id { get; set; }
        public int BrokerId { get; set; }
        public string Broker { get; set; }
        /// <summary>Statement month this credit pays for. Empty = not allocated (e.g. paid via another firm).</summary>
        public DateTime? StatementMonth { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public decimal AmountReceived { get; set; }
        public string PaidByFirm { get; set; }
        public decimal? TdsRate { get; set; }
        public string Remarks { get; set; }
        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }

        // calculated
        public decimal? CommissionPerStatement { get; set; }
        public decimal? Tds { get; set; }
        public decimal? ShortBy { get; set; }
        public decimal? Unexplained { get; set; }
        public decimal? PercentReceived { get; set; }
    }
}

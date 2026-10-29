namespace PolicyTrackerWebService.Domain
{
    /// <summary>Broker payout grid: points by category and eldest insured age band.</summary>
    public class PointsGrid
    {
        public int Id { get; set; }
        public int BrokerId { get; set; }
        public string Broker { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int AgeFrom { get; set; }
        public int AgeTo { get; set; }
        public decimal Points { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }
        public string Status { get; set; }
    }
}

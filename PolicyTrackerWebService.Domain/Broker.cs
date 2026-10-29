namespace PolicyTrackerWebService.Domain
{
    public class Broker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string SalesManager { get; set; }
        public string ContactNo { get; set; }
        public string EmailAddress { get; set; }
        /// <summary>TDS % the broker deducts (e.g. 5 = 5%).</summary>
        public decimal TdsRate { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }
        public string Status { get; set; }
    }
}

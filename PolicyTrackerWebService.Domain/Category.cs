namespace PolicyTrackerWebService.Domain
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>Default points (commission %) on Net premium when no age-band row matches.</summary>
        public decimal? DefaultPoints { get; set; }
        /// <summary>Motor categories are paid on OD / TP instead of Net.</summary>
        public bool IsMotor { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }
        public string Status { get; set; }
    }
}

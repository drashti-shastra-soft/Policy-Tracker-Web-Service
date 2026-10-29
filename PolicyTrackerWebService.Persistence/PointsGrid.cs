using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("PointsGrids")]
    public class PointsGrid : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int BrokerId { get; set; }

        public int CategoryId { get; set; }

        public int AgeFrom { get; set; }

        public int AgeTo { get; set; }

        public decimal Points { get; set; }

        public bool IsActive { get; set; }
    }
}

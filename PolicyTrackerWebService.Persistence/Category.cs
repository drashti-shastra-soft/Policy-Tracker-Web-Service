using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("Categories")]
    public class Category : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        public decimal? DefaultPoints { get; set; }

        public bool IsMotor { get; set; }

        public bool IsActive { get; set; }
    }
}

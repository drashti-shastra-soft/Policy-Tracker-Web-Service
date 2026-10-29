using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("Brokers")]
    public class Broker : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(50)]
        public string PosCode { get; set; }

        [StringLength(150)]
        public string PosName { get; set; }

        [StringLength(150)]
        public string SalesManager { get; set; }

        [StringLength(20)]
        public string ContactNo { get; set; }

        [StringLength(150)]
        public string EmailAddress { get; set; }

        public decimal TdsRate { get; set; }

        public bool IsActive { get; set; }
    }
}

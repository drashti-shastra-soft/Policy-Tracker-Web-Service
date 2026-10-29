using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("InsuranceCompanies")]
    public class InsuranceCompany : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [StringLength(50)]
        public string ShortName { get; set; }

        public bool IsActive { get; set; }
    }
}

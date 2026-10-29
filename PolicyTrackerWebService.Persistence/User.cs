using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("Users")]
    public class User : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        [Required, StringLength(150)]
        public string EmailAddress { get; set; }

        [Required, StringLength(200)]
        public string PasswordHash { get; set; }

        [StringLength(20)]
        public string MobileNo { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}

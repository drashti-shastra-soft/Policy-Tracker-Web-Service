using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyTrackerWebService.Persistence
{
    [Table("Payouts")]
    public class Payout : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int BrokerId { get; set; }

        public DateTime? StatementMonth { get; set; }

        public DateTime? ReceivedOn { get; set; }

        public decimal AmountReceived { get; set; }

        [StringLength(150)]
        public string PaidByFirm { get; set; }

        public decimal? TdsRate { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }
    }
}

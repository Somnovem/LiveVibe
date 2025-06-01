using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiveVibe.Server.Models.Tables
{
    [Table("Ticket_Purchases")]
    public class TicketPurchase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }

        [Required]
        [ForeignKey("TicketId")]
        public Guid TicketId { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool WasRefunded { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal PurchasePrice { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Ticket Ticket { get; set; } = null!;
    }
}

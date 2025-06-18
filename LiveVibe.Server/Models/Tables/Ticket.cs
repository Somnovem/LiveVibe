using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiveVibe.Server.Models.Tables
{
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Event")]
        public Guid EventId { get; set; }

        [Required]
        [ForeignKey("SeatingCategory")]
        public Guid SeatingCategoryId { get; set; }

        [Required]
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Seat { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string? QRCodeSvg { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool WasRefunded { get; set; }

        public virtual Event Event { get; set; } = null!;
        public virtual EventSeatType SeatingCategory { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}

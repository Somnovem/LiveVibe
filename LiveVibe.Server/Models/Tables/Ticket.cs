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
        [Column(TypeName = "varchar(50)")]
        public string Seat { get; set; } = string.Empty;

        [Required]
        [ForeignKey("SeatingCategory")]
        public Guid SeatingCategoryId { get; set; }

        public virtual Event Event { get; set; } = null!;
        public virtual EventSeatType SeatingCategory { get; set; } = null!;
    }
}

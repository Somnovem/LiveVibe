using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiveVibe.Server.Models.Tables
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal TotalPrice { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool WasRefunded { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace LiveVibe.Server.Models.Tables
{
    [Table("AspNetUsers")]
    public class User : IdentityUser<Guid>
    {
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(512)")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<TicketPurchase> TicketPurchases { get; set; } = new List<TicketPurchase>();
    }
}
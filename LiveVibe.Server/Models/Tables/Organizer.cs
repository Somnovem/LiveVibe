using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.Tables
{
    [Table("Organizers")]
    public class Organizer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(255)")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiveVibe.Server.Models.Tables
{
    [Table("Events")]
    public class Event
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(1000)")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "uniqueidentifier")]
        public Guid OrganizerId { get; set; }

        [Required]
        [ForeignKey("EventCategory")] 
        [Column(TypeName = "uniqueidentifier")]
        public Guid CategoryId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "uniqueidentifier")]
        public Guid CityId { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? ImageUrl { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; }

        public virtual Organizer Organizer { get; set; } = null!;
        public virtual EventCategory EventCategory { get; set; } = null!;
        public virtual City City { get; set; } = null!;
        public virtual ICollection<EventSeatType> EventSeatTypes { get; set; } = new List<EventSeatType>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

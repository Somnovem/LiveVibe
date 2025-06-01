using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiveVibe.Server.Models.DTOs.Requests.EventSeatTypes
{
    public class UpdateEventSeatTypeRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [Required]
        public int AvailableSeats { get; set; }

        [Required]
        [Column(TypeName = "float")]
        public double Price { get; set; }
    }
}

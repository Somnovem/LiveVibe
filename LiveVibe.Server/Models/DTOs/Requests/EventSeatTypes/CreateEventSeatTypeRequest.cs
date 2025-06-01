using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.EventSeatTypes
{
    public class CreateEventSeatTypeRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [Required]
        public int AvailableSeats { get; set; }

        [Required]
        public double Price { get; set; }
    }
}

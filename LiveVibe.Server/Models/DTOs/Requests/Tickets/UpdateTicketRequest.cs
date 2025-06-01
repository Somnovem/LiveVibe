using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Tickets
{
    public class UpdateTicketRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [Required, MaxLength(50)]
        public string Seat { get; set; } = string.Empty;

        [Required]
        public Guid SeatingCategoryId { get; set; }
    }
}
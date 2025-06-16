using LiveVibe.Server.Models.DTOs.ModelDTOs;

namespace LiveVibe.Server.Models.DTOs.Models
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public required List<TicketDTO> Tickets { get; set; }
    }
}

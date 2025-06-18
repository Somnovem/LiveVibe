using LiveVibe.Server.Models.DTOs.Models;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class TicketDTO
    {
        public required Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public Guid SeatTypeId { get; set; }
        public string Seat { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string QRCodeUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}

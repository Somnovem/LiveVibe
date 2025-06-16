using LiveVibe.Server.Models.DTOs.Models;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class TicketDTO
    {
        public required Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid SeatTypeId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string QRCodeUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}

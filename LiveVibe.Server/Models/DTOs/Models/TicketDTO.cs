using LiveVibe.Server.Models.DTOs.Models;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class TicketDTO
    {
        public Guid Id { get; set; }
        public required ShortEventDTO Event { get; set; }
        public string SeatingCategoryType { get; set; } = string.Empty;
        public string Seat { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool WasRefunded { get; set; }
    }

}

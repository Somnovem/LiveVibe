using LiveVibe.Server.Models.Tables;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class ShortEventDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Organizer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string? ImageUrl { get; set; }
        public string? MatchedSeatCategoryName { get; set; }
        public decimal? MatchedSeatCategoryPrice { get; set; }
    }
}

namespace LiveVibe.Server.Models.DTOs.Models
{
    public class ShortEventDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string? ImageUrl { get; set; } = string.Empty;
    }
}

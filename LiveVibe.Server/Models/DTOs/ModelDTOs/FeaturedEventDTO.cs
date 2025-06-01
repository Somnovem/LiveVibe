using LiveVibe.Server.Models.Tables;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class FeaturedEventDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Organizer { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string Location { get; set; } = null!;
        public DateTime Time { get; set; }
        public string? ImageUrl { get; set; }

        public FeaturedEventDTO(Event _event)
        {
            {
                Id = _event.Id;
                Title = _event.Title;
                Description = !string.IsNullOrEmpty(_event.Description)
                    ? (_event.Description.Length > 100 ? _event.Description.Substring(0, 100) + "..." : _event.Description)
                    : null;
                Organizer = _event.Organizer.Name;
                Category = _event.EventCategory.Name;
                Country = _event.Country.Name;
                Location = _event.Location;
                Time = _event.Time;
                ImageUrl = _event.ImageUrl;
            }
        }
    }
} 
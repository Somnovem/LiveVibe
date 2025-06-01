namespace LiveVibe.Server.Models.DTOs.Requests.Events
{
    public class EventSearchRequest
    {
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? City { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

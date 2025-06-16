namespace LiveVibe.Server.Models.DTOs.Responses
{
    public class TicketVerificationResponseDTO
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public TicketVerificationDetails TicketDetails { get; set; } = null!;
    }

    public class TicketVerificationDetails
    {
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string Seat { get; set; } = string.Empty;
        public string SeatingCategory { get; set; } = string.Empty;
    }
}

using LiveVibe.Server.Models.Tables;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class ShortTicketDTO
    {
        public Guid Id { get; set; }
        public string Seat { get; set; } = string.Empty;

        public ShortTicketDTO(Ticket ticket)
        {
            Id = ticket.Id;
            Seat = ticket.Seat;
        }
    }
}

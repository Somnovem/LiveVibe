namespace LiveVibe.Server.Models.DTOs.Models;

public class EventSeatTypeDTO
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int AvailableSeats { get; set; }
    public double Price { get; set; }
}
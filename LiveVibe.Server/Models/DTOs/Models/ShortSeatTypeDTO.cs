using LiveVibe.Server.Models.Tables;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class ShortSeatTypeDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AvailableSeats { get; set; }
        public int Capacity { get; set; }
        public double Price { get; set; }

        public ShortSeatTypeDTO(EventSeatType eventSeatType)
        {
            Id = eventSeatType.Id;
            Name = eventSeatType.Name;
            AvailableSeats = eventSeatType.AvailableSeats;
            Capacity = eventSeatType.Capacity;
            Price = eventSeatType.Price;
        }
    }
}

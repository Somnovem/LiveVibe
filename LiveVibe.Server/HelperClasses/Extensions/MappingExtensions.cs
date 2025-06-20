using LiveVibe.Server.Models.DTOs.Models;
using LiveVibe.Server.Models.Tables;

namespace LiveVibe.Server.HelperClasses.Extensions;

public static class MappingExtensions
{
    public static EventSeatTypeDTO ToDto(this EventSeatType entity)
    {
        return new EventSeatTypeDTO
        {
            Id = entity.Id,
            EventId = entity.EventId,
            Name = entity.Name,
            Capacity = entity.Capacity,
            AvailableSeats = entity.AvailableSeats,
            Price = entity.Price
        };
    }
}
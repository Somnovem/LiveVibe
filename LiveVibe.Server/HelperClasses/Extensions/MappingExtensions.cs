using LiveVibe.Server.Models.DTOs.ModelDTOs;
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

    public static UserDTO ToDto(this User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Email = user.Email!,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    } 

    public static UserExtendedDTO ToExtendedDto(this User user)
    {
        return new UserExtendedDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Email = user.Email!,
            Orders = user.Orders.Select(o => o.ToDto(user)).ToList()
        };
    }

    public static TicketDTO ToDto(this Ticket t)
    {
        return new TicketDTO
        {
            Id = t.Id,
            EventId = t.EventId,
            EventName = t.Event?.Title ?? string.Empty,
            SeatTypeId = t.SeatingCategoryId,
            Seat = t.Seat,
            OrderId = t.OrderId,
            QRCodeUrl = $"http://localhost:5000/api/tickets/{t.Id}/qrcode",
            CreatedAt = t.CreatedAt
        };
    }

    public static OrderDTO ToDto(this Order order, User user)
    {
        return new OrderDTO
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.WasRefunded ? "Refunded" : "Paid",
            CreatedAt = order.CreatedAt,
            Firstname = user.FirstName,
            Lastname = user.LastName,
            Email = user.Email!,
            Tickets = order.Tickets.Select(t => t.ToDto()).ToList()
        };
    }
}
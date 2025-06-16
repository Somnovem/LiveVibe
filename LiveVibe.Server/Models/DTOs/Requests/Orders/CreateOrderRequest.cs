using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid SeatTypeId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required, MaxLength(100)]
        public required string FirstName { get; set; }

        [Required, MaxLength(100)]
        public required string LastName { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public required string Email { get; set; }
    }
}

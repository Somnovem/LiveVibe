using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.TicketPurchases
{
    public class PurchaseTicketRequest
    {
        [Required(ErrorMessage = "TicketId is required")]
        public Guid TicketId { get; set; }
    }
} 
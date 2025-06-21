using System.Security.Claims;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using LiveVibe.Server.HelperClasses.Extensions;
using LiveVibe.Server.Models.DTOs.Shared;
using LiveVibe.Server.Models.DTOs.Responses;
using LiveVibe.Server.Models.DTOs.Requests.Orders;
using LiveVibe.Server.Services;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/orders")]
    public class OrderController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IQRCodeService _qrCodeService;

        public OrderController(ApplicationContext db, UserManager<User> userManager, IEmailService emailService, IQRCodeService qrCodeService)
        {
            _context = db;
            _userManager = userManager;
            _emailService = emailService;
            _qrCodeService = qrCodeService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Admin] Retrieve all orders.", Description = "Returns a list of all orders in the database.")]
        [SwaggerResponse(200, "Success", typeof(PagedListDTO<Order>))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        public async Task<ActionResult<PagedListDTO<Order>>> GetOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var orders = await _context.Orders
                                    .AsNoTracking()
                                    .ToPagedListAsync(pageNumber, pageSize);
            
            return Ok(orders.ToDto());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "[Admin] Get Order by ID.")]
        [SwaggerResponse(200, "Order found.", typeof(Order))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "Order not found.", typeof(ErrorDTO))]
        public async Task<ActionResult<Order>> GetOrderById(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Tickets)
                .ThenInclude(t => t.SeatingCategory)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new ErrorDTO("Order not found."));

            return Ok(order);
        }

        [Authorize(Roles = "User")]
        [HttpPost("create")]
        [SwaggerOperation(
            Summary = "[User] Place an order for tickets.",
            Description = "Allows a user with the 'User' role to place an order for tickets."
        )]
        [SwaggerResponse(200, "Order created successfully.", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Bad request (e.g., event already happened, ticket already purchased, no seats available).", typeof(ErrorDTO))]
        [SwaggerResponse(401, "Unauthorized: invalid or missing user credentials.", typeof(ErrorDTO))]
        [SwaggerResponse(404, "User, Event or EventSeatType were not found.", typeof(ErrorDTO))]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials."));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new ErrorDTO("User not found."));

            var _event = await _context.Events.FirstOrDefaultAsync(e => e.Id == request.EventId);

            if (_event == null)
                return NotFound(new ErrorDTO("Event not found."));

            if (_event.Time <= DateTime.UtcNow)
                return BadRequest(new ErrorDTO("Event already happened."));

            var _eventSeatType = await _context.EventSeatTypes.FirstOrDefaultAsync(e => e.Id == request.SeatTypeId);

            if (_eventSeatType == null)
                return NotFound(new ErrorDTO("Seating type not found."));

            if (_eventSeatType.AvailableSeats < request.Quantity)
                return BadRequest(new ErrorDTO("Not enough available seats to fulfill request."));

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email!,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = Convert.ToDecimal(_eventSeatType.Price * request.Quantity),
                WasRefunded = false
            };

            _context.Orders.Add(order);

            var tickets = new List<Ticket>();

            var refundedTickets = await _context.Tickets
                .Where(t => t.SeatingCategoryId == _eventSeatType.Id && t.WasRefunded)
                .Take(request.Quantity)
                .ToListAsync();

            var repurchasedTickets = new List<Ticket>();
            foreach (var ticket in refundedTickets)
            {
                ticket.OrderId = order.Id;
                ticket.WasRefunded = false;
                repurchasedTickets.Add(ticket);
            }

            // If we need more tickets, create new ones
            var remainingTickets = request.Quantity - refundedTickets.Count;
            if (remainingTickets > 0) 
            {
                var lastSeatNumber = tickets
                    .Select(t => int.Parse(t.Seat.Split("-", StringSplitOptions.None).Last()))
                    .DefaultIfEmpty(0)
                    .Max();
                var seatNumber = lastSeatNumber + 1;

                for (int i = 1; i <= remainingTickets; i++)
                {
                    var ticketId = Guid.NewGuid();
                    var ticketUrl = $"http://localhost:5000/api/tickets/{ticketId}";
                    var qrCodeSvg = _qrCodeService.GenerateQRCodeSvg(ticketUrl);

                    tickets.Add(new Ticket
                    {
                        Id = ticketId,
                        OrderId = order.Id,
                        SeatingCategoryId = _eventSeatType.Id,
                        EventId = _event.Id,
                        Price = Convert.ToDecimal(_eventSeatType.Price),
                        WasRefunded = false,
                        Seat = $"{_eventSeatType.Name}-{seatNumber:D3}",
                        QRCodeSvg = qrCodeSvg,
                        CreatedAt = DateTime.UtcNow,
                    });
                    seatNumber++;
                }

            }

            _context.Tickets.AddRange(tickets);
            _eventSeatType.AvailableSeats -= tickets.Count;
            await _context.SaveChangesAsync();

            tickets.AddRange(repurchasedTickets);

            var ticketListHtml = string.Join("", tickets.Select(t => $@"
                <div style='background-color: #f8f9fa; padding: 15px; margin: 10px 0; border-radius: 5px; border-left: 4px solid #007bff;'>
                    <div style='font-weight: bold; color: #007bff;'>Seat {t.Seat}</div>
                    <div style='color: #28a745;'>Price: {t.Price} UAH</div>
                </div>"));

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #007bff; color: white; padding: 20px; border-radius: 5px 5px 0 0;'>
                        <h1 style='margin: 0;'>Order Confirmation</h1>
                    </div>
                    <div style='background-color: white; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                        <p style='font-size: 18px;'>Hi {order.FirstName},</p>
                        <p style='font-size: 16px;'>Thank you for your order for <strong style='color: #007bff;'>{_event.Title}</strong>.</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; margin: 15px 0; border-radius: 5px;'>
                            <p style='margin: 5px 0;'><strong>Event Date:</strong> {_event.Time:yyyy-MM-dd HH:mm}</p>
                            <p style='margin: 5px 0;'><strong>Total Price:</strong> <span style='color: #28a745;'>{order.TotalPrice} UAH</span></p>
                            <p style='margin: 5px 0;'><strong>Order ID:</strong> {order.Id}</p>
                        </div>

                        <h3 style='color: #007bff; margin-top: 20px;'>Your Tickets</h3>
                        {ticketListHtml}

                        <div style='margin-top: 20px; padding-top: 20px; border-top: 1px solid #dee2e6;'>
                            <p style='color: #6c757d; font-size: 14px;'>Thank you for choosing LiveVibe!</p>
                        </div>
                        <div style='text-align: center; margin-top: 20px; color: #999; font-size: 12px;'>
                            <p>This is an automated message, please do not reply directly to this email.</p>
                        </div>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(order.Email, $"Order Confirmation: Order #{order.Id}", emailBody);

            return Ok(new SuccessDTO("Order created successfully."));
        }

        [Authorize(Roles = "User")]
        [HttpPatch("refund-order/{id}")]
        [SwaggerOperation(Summary = "[User] Refund an entire Order.", Description = "Refunds all tickets in an order, if performed by the user who purchased it.")]
        [SwaggerResponse(200, "Order refunded successfully", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Invalid input or event already happened")]
        [SwaggerResponse(401, "Unauthorized: invalid or missing user credentials.", typeof(ErrorDTO))]
        [SwaggerResponse(404, "Order not found", typeof(ErrorDTO))]
        public async Task<IActionResult> RefundOrder(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user ID missing in token)."));
            }

            var order = await _context.Orders
                .Include(o => o.Tickets)
                .ThenInclude(t => t.SeatingCategory)
                .ThenInclude(s => s.Event)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new ErrorDTO("Order not found."));

            if (order.UserId != Guid.Parse(userId))
                return Unauthorized(new ErrorDTO("Order belongs to another user."));

            if (order.WasRefunded)
                return BadRequest(new ErrorDTO("Order has already been refunded."));

            if (order.Tickets.Any(t => t.SeatingCategory.Event.Time <= DateTime.UtcNow))
                return BadRequest(new ErrorDTO("Cannot refund order containing tickets for events that have already happened."));

            foreach (var ticket in order.Tickets)
            {
                // placeholder for actual refundal logic
                ticket.WasRefunded = true;
            }

            order.WasRefunded = true;
            foreach (var ticketInOrder in order.Tickets)
            {
                ticketInOrder.SeatingCategory.AvailableSeats += 1;
            }
            await _context.SaveChangesAsync();

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <h2 style='color: #333; text-align: center;'>Order Refund Confirmation</h2>
                    <p style='color: #666;'>Dear {order.Email},</p>
                    <p style='color: #666;'>Your order #{order.Id} has been successfully refunded.</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='color: #333; margin-top: 0;'>Refund Details:</h3>
                        <p style='color: #666; margin: 5px 0;'><strong>Order ID:</strong> {order.Id}</p>
                        <p style='color: #666; margin: 5px 0;'><strong>Refund Date:</strong> {DateTime.UtcNow.ToString("g")}</p>
                        <p style='color: #666; margin: 5px 0;'><strong>Number of Tickets:</strong> {order.Tickets.Count}</p>
                    </div>
                    <p style='color: #666;'>If you have any questions about your refund, please contact our support team.</p>
                    <p style='color: #666;'>Thank you for choosing LiveVibe!</p>
                    <div style='text-align: center; margin-top: 20px; color: #999; font-size: 12px;'>
                        <p>This is an automated message, please do not reply directly to this email.</p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(order.Email, $"Order Refund Confirmation: Order #{order.Id}", emailBody);

            return Ok(new SuccessDTO("Order refunded successfully."));
        }

        [Authorize(Roles = "User")]
        [HttpPatch("refund/{id}")]
        [SwaggerOperation(Summary = "[User] Refund a Ticket.", Description = "Refunds a ticket with provided Id, if performed by the user who purchased it.")]
        [SwaggerResponse(200, "Ticket refunded successfully", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Invalid input or event already happened")]
        [SwaggerResponse(401, "Unauthorized: invalid or missing user credentials.", typeof(ErrorDTO))]
        [SwaggerResponse(404, "Ticket not found", typeof(ErrorDTO))]
        public async Task<IActionResult> RefundTicketPurchase(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user ID missing in token)."));
            }

            var ticket = await _context.Tickets
                .Include(t => t.Order)
                .Include(t => t.SeatingCategory)
                .ThenInclude(s => s.Event)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound(new ErrorDTO("Ticket not found."));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user no longer exists)."));
            }

            if (ticket.Order.UserId != Guid.Parse(userId))
            {
                return Unauthorized(new ErrorDTO("Ticket belongs to another user."));
            }

            if (ticket.WasRefunded)
            {
                return BadRequest(new ErrorDTO("Ticket has already been refunded."));
            }

            if (ticket.SeatingCategory.Event.Time <= DateTime.UtcNow)
            {
                return BadRequest(new ErrorDTO("Cannot refund ticket for an event that has already happened."));
            }

            // placeholder for actual refundal logic
            ticket.WasRefunded = true;

            var order = ticket.Order;
            var allTicketsRefunded = await _context.Tickets
                .Where(t => t.OrderId == order.Id)
                .AllAsync(t => t.WasRefunded);

            if (allTicketsRefunded)
            {
                order.WasRefunded = true;
            }

            ticket.SeatingCategory.AvailableSeats += 1;

            await _context.SaveChangesAsync();

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <h2 style='color: #333; text-align: center;'>Ticket Refund Confirmation</h2>
                    <p style='color: #666;'>Dear {order.FirstName},</p>
                    <p style='color: #666;'>Your ticket #{ticket.Id} has been successfully refunded.</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='color: #333; margin-top: 0;'>Refund Details:</h3>
                        <p style='color: #666; margin: 5px 0;'><strong>Order ID:</strong> {order.Id}</p>
                        <p style='color: #666; margin: 5px 0;'><strong>Ticket ID:</strong> {ticket.Id}</p>
                        <p style='color: #666; margin: 5px 0;'><strong>Refund Date:</strong> {DateTime.UtcNow.ToString("g")}</p>
                    </div>
                    <p style='color: #666;'>If you have any questions about your refund, please contact our support team.</p>
                    <p style='color: #666;'>Thank you for choosing LiveVibe!</p>
                    <div style='text-align: center; margin-top: 20px; color: #999; font-size: 12px;'>
                        <p>This is an automated message, please do not reply directly to this email.</p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(order.Email, $"Ticket Refund Confirmation: Order #{order.Id}", emailBody);

            return Ok(new SuccessDTO("Ticket refunded successfully."));
        }
    }
}

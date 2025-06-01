using System.Security.Claims;
using LiveVibe.Server.Models.DTOs.Requests.TicketPurchases;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/ticket-purchases")]
    public class TicketPurchaseController : Controller
    {

        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;

        public TicketPurchaseController(ApplicationContext db, UserManager<User> userManager)
        {
            _context = db;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Admin] Retrieve all ticket purchases.", Description = "Returns a list of all ticket purchases in the database.")]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<TicketPurchase>))]
        public async Task<IEnumerable<TicketPurchase>> GetTicketPurchasePurchases(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var ticketPurchases = await _context.TicketPurchases
                                            .AsNoTracking()
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            var totalTicketPurchases = await _context.TicketPurchases.CountAsync();
            var totalPages = (int)Math.Ceiling(totalTicketPurchases / (double)pageSize);

            Response.Headers.Append("X-Total-Count", totalTicketPurchases.ToString());
            Response.Headers.Append("X-Total-Pages", totalPages.ToString());

            return ticketPurchases;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "[Admin] Get TicketPurchase by ID.")]
        [SwaggerResponse(200, "TicketPurchase found.", typeof(TicketPurchase))]
        [SwaggerResponse(404, "TicketPurchase not found.")]
        public async Task<ActionResult<TicketPurchase>> GetTicketPurchaseById(Guid id)
        {
            var ticketPurchase = await _context.TicketPurchases.FindAsync(id);
            if (ticketPurchase == null)
                return NotFound();

            return Ok(ticketPurchase);
        }

        //[Authorize(Roles = "Admin")]
        //[HttpPost("create")]
        //[SwaggerOperation(Summary = "Create a new TicketPurchase.", Description = "If provided with valid data, create a new TicketPurchase.")]
        //[SwaggerResponse(201, "TicketPurchase created successfully.", typeof(TicketPurchase))]
        //[SwaggerResponse(400, "Invalid input.")]
        //[SwaggerResponse(409, "Same TicketPurchase already exists")]
        //public ActionResult<TicketPurchase> CreateTicketPurchase([FromBody] CreateTicketPurchaseRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    bool ticketExists = _context.Tickets.Any(t => t.Id == request.TicketId);
        //    if (!ticketExists)
        //    {
        //        return BadRequest("Ticket with this Id doesn't exist.");
        //    }

        //    bool userExists = _context.EventSeatTypes.Any(u => u.Id == request.UserId);
        //    if (!userExists)
        //    {
        //        return BadRequest("User with this Id doesn't exist.");
        //    }

        //    bool ticketPurchaseExists = _context.TicketPurchases.Any(t => t.TicketId == request.TicketId
        //                                            && t.UserId == request.UserId);
        //    if (ticketPurchaseExists)
        //    {
        //        return Conflict("This TicketPurchase already exists.");
        //    }

        //    var ticketPurchase = new TicketPurchase
        //    {
        //        Id = Guid.NewGuid(),
        //        UserId = request.UserId,
        //        TicketId = request.TicketId,
        //        CreatedAt = DateTime.UtcNow,
        //        WasRefunded = false
        //    };

        //    _context.TicketPurchases.Add(ticketPurchase);
        //    _context.SaveChanges();

        //    return CreatedAtAction(
        //        nameof(CreateTicketPurchaseRequest),
        //        new { id = ticketPurchase.Id },
        //        ticketPurchase
        //    );
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpPut("update")]
        //[SwaggerOperation(Summary = "Update an existing TicketPurchase.", Description = "Updates existing TicketPurchase details.")]
        //[SwaggerResponse(200, "TicketPurchase updated successfully", typeof(TicketPurchase))]
        //[SwaggerResponse(400, "Invalid input")]
        //[SwaggerResponse(404, "TicketPurchase not found")]
        //[SwaggerResponse(409, "Same TicketPurchase already exists")]
        //public ActionResult<TicketPurchase> UpdateTicketPurchase([FromBody] UpdateTicketPurchaseRequest request)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var ticketPurchase = _context.TicketPurchases.FirstOrDefault(t => t.Id == request.Id);
        //    if (ticketPurchase == null)
        //        return NotFound($"No TicketPurchase found with ID {request.Id}");

        //    bool ticketExists = _context.Tickets.Any(t => t.Id == request.TicketId);
        //    if (!ticketExists)
        //    {
        //        return BadRequest("Ticket with this Id doesn't exist.");
        //    }

        //    bool userExists = _context.EventSeatTypes.Any(u => u.Id == request.UserId);
        //    if (!userExists)
        //    {
        //        return BadRequest("User with this Id doesn't exist.");
        //    }

        //    bool ticketPurchaseExists = _context.TicketPurchases.Any(t => t.TicketId == request.TicketId
        //                                            && t.UserId == request.UserId);
        //    if (ticketPurchaseExists)
        //    {
        //        return Conflict("This TicketPurchase already exists.");
        //    }

        //    ticketPurchase.UserId = request.UserId;
        //    ticketPurchase.TicketId = request.TicketId;

        //    _context.TicketPurchases.Update(ticketPurchase);
        //    _context.SaveChanges();

        //    return Ok(ticketPurchase);
        //}

        [Authorize(Roles = "User")]
        [HttpPost("purchase")]
        [SwaggerOperation(
            Summary = "[User] Purchase a ticket.",
            Description = "Allows a user with the 'User' role to purchase a ticket by providing its ID."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Ticket purchased successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request (e.g., event already happened, ticket already purchased, no seats available).")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized: invalid or missing user credentials.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ticket not found.")]
        public async Task<IActionResult> PurchaseTicket([FromBody] Guid ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized("Invalid credentials.");
            }

            var ticket = await _context.Tickets
                .Include(t => t.SeatingCategory)
                .Include(t => t.SeatingCategory.Event)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound("Ticket not found.");

            if (ticket.SeatingCategory.Event.Time <= DateTime.UtcNow)
                return BadRequest("Event already happened.");

            if (await _context.TicketPurchases.AnyAsync(t => t.TicketId == ticketId && !t.WasRefunded))
                return BadRequest("Ticket already purchased.");

            var seatType = ticket.SeatingCategory;
            if (seatType.AvailableSeats <= 0)
                return BadRequest("No seats available in this category.");

            var purchase = new TicketPurchase
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                TicketId = ticket.Id,
                CreatedAt = DateTime.UtcNow,
                WasRefunded = false,
                PurchasePrice = Convert.ToDecimal(seatType.Price)
            };

            _context.TicketPurchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Ok("Ticket purchased successfully.");
        }


        [Authorize(Roles = "User")]
        [HttpPatch("refund/{id}")]
        [SwaggerOperation(Summary = "[User] Refund a Ticket Purchase.", Description = "Refunds the ticket purchase with provided Id, if performed by the user who done it.")]
        [SwaggerResponse(200, "Ticket refunded successfully")]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "TicketPurchase not found")]
        public async Task<IActionResult> RefundTicketPurchase(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized("Invalid credentials (user ID missing in token).");
            }

            var ticketPurchase = _context.TicketPurchases.FirstOrDefault(t => t.Id == id);
            if (ticketPurchase == null)
                return NotFound("TicketPurchase not found.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized("Invalid credentials (user no longer exists).");
            }

            if (ticketPurchase.UserId != user.Id)
            {
                return Unauthorized("Ticket belongs to another user.");
            }

            ticketPurchase.WasRefunded = true;
            _context.SaveChanges();

            return Ok("Ticket refunded successfully.");
        }

    }
}

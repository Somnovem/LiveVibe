using System.Security.Claims;
using LiveVibe.Server.Models.DTOs.Requests.Tickets;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using LiveVibe.Server.Models.DTOs.ModelDTOs;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/tickets")]
    public class TicketController : Controller
    {

        private readonly ApplicationContext _context;

        public TicketController(ApplicationContext db)
        {
            _context = db;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Admin] Retrieve all tickets.", Description = "Returns a list of all tickets in the database.")]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<Ticket>))]
        public async Task<IEnumerable<Ticket>> GetTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var tickets = await _context.Tickets
                                            .AsNoTracking()
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            var totalTickets = await _context.Tickets.CountAsync();
            var totalPages = (int)Math.Ceiling(totalTickets / (double)pageSize);

            Response.Headers.Append("X-Total-Count", totalTickets.ToString());
            Response.Headers.Append("X-Total-Pages", totalPages.ToString());

            return tickets;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "[Admin] Get Ticket by ID.")]
        [SwaggerResponse(200, "Ticket found.", typeof(Ticket))]
        [SwaggerResponse(404, "Ticket not found.")]
        public async Task<ActionResult<Ticket>> GetTicketById(Guid id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        //[Authorize(Roles = "Admin")]
        //[HttpPost("create")]
        //[SwaggerOperation(Summary = "Create a new Ticket.", Description = "If provided with valid data, create a new Ticket.")]
        //[SwaggerResponse(201, "Ticket created successfully.", typeof(Ticket))]
        //[SwaggerResponse(400, "Invalid input.")]
        //[SwaggerResponse(409, "Same Ticket already exists")]
        //public async Task<ActionResult<Ticket>> CreateTicket([FromBody] CreateTicketRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    bool eventExists = await _context.Events.AnyAsync(o => o.Id == request.EventId);
        //    if (!eventExists)
        //    {
        //        return BadRequest("Event with this Id doesn't exist.");
        //    }

        //    bool seatingCategoryExists = await _context.EventSeatTypes.AnyAsync(e => e.Id == request.SeatingCategoryId);
        //    if (!seatingCategoryExists)
        //    {
        //        return BadRequest("Seating Category with this Id doesn't exist.");
        //    }

        //    bool ticketExists = await _context.Tickets.AnyAsync(e => e.EventId == request.EventId
        //                                            && e.SeatingCategoryId == request.SeatingCategoryId
        //                                            && e.Seat == request.Seat.Trim());
        //    if (ticketExists)
        //    {
        //        return Conflict("This Ticket already exists.");
        //    }

        //    var ticket = new Ticket
        //    {
        //        Id = Guid.NewGuid(),
        //        EventId = request.EventId,
        //        Seat = request.Seat.Trim(),
        //        SeatingCategoryId = request.SeatingCategoryId
        //    };

        //    _context.Tickets.Add(ticket);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(
        //        nameof(GetTicketById),
        //        new { id = ticket.Id },
        //        ticket
        //    );
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpPut("update")]
        //[SwaggerOperation(Summary = "Update an existing Ticket.", Description = "Updates existing Ticket details.")]
        //[SwaggerResponse(200, "Ticket updated successfully", typeof(Ticket))]
        //[SwaggerResponse(400, "Invalid input")]
        //[SwaggerResponse(404, "Ticket not found")]
        //[SwaggerResponse(409, "Same Ticket already exists")]
        //public async Task<ActionResult<Ticket>> UpdateTicket([FromBody] UpdateTicketRequest request)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var ticket = await _context.Tickets.FirstOrDefaultAsync(e => e.Id == request.Id);
        //    if (ticket == null)
        //        return NotFound($"No Ticket found with ID {request.Id}");

        //    bool eventExists = await _context.Events.AnyAsync(o => o.Id == request.EventId);
        //    if (!eventExists)
        //    {
        //        return BadRequest("Event with this Id doesn't exist.");
        //    }

        //    bool seatingCategoryExists = await _context.EventSeatTypes.AnyAsync(e => e.Id == request.SeatingCategoryId);
        //    if (!seatingCategoryExists)
        //    {
        //        return BadRequest("Seating Category with this Id doesn't exist.");
        //    }

        //    bool ticketExists = await _context.Tickets.AnyAsync(e => e.EventId == request.EventId
        //                                            && e.SeatingCategoryId == request.SeatingCategoryId
        //                                            && e.Seat == request.Seat.Trim());
        //    if (ticketExists)
        //    {
        //        return Conflict("This Ticket already exists.");
        //    }

        //    ticket.EventId = request.EventId;
        //    ticket.Seat = request.Seat.Trim();
        //    ticket.SeatingCategoryId = request.SeatingCategoryId;

        //    _context.Tickets.Update(ticket);
        //    await _context.SaveChangesAsync();

        //    return Ok(ticket);
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpDelete("delete/{id}")]
        //[SwaggerOperation(Summary = "Delete a Ticket.", Description = "Deletes the Ticket with the specified ID.")]
        //[SwaggerResponse(200, "Ticket deleted successfully")]
        //[SwaggerResponse(400, "Invalid input")]
        //[SwaggerResponse(404, "Ticket not found")]
        //public async Task<IActionResult> DeleteTicket(Guid id)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var ticket = await _context.Tickets.FirstOrDefaultAsync(e => e.Id == id);
        //    if (ticket == null)
        //        return NotFound("Ticket not found.");

        //    _context.Tickets.Remove(ticket);
        //    await _context.SaveChangesAsync();

        //    return Ok("Ticket deleted successfully.");
        //}
    }
}

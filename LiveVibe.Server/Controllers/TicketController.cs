using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using LiveVibe.Server.HelperClasses.Extensions;
using LiveVibe.Server.Models.DTOs.Shared;

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
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        public async Task<ActionResult<PagedListDTO<Ticket>>> GetTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var tickets = await _context.Tickets
                                            .AsNoTracking()
                                            .ToPagedListAsync(pageNumber, pageSize);

            return Ok(tickets.ToDto());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "[Admin] Get Ticket by ID.")]
        [SwaggerResponse(200, "Ticket found.", typeof(Ticket))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "Ticket not found.", typeof(ErrorDTO))]
        public async Task<ActionResult<Ticket>> GetTicketById(Guid id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new ErrorDTO("Ticket not found."));

            return Ok(ticket);
        }
    }
}

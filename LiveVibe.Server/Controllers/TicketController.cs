using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using LiveVibe.Server.HelperClasses.Extensions;
using LiveVibe.Server.Models.DTOs.Shared;
using LiveVibe.Server.Services;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/tickets")]
    public class TicketController : Controller
    {

        private readonly ApplicationContext _context;
        private readonly IQRCodeService _qrCodeService;

        public TicketController(ApplicationContext db, IQRCodeService qrCodeService)
        {
            _context = db;
            _qrCodeService = qrCodeService;
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

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "[Any] Get Ticket by ID.")]
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

        [HttpGet("{id}/qrcode")]
        [SwaggerOperation(Summary = "Get QR code for a ticket.")]
        [SwaggerResponse(200, "QR code found.", typeof(string))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated.")]
        [SwaggerResponse(404, "Ticket not found.", typeof(ErrorDTO))]
        public async Task<ActionResult<string>> GetTicketQRCode(Guid id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new ErrorDTO("Ticket not found."));

            // The QR code for this ticket has not been generated yet.
            if (string.IsNullOrEmpty(ticket.QRCodeSvg))
            {
                var ticketUrl = $"http://localhost:5000/api/tickets/{id}";

                var qrCodeSvg = _qrCodeService.GenerateQRCodeSvg(ticketUrl);

                ticket.QRCodeSvg = qrCodeSvg;
                await _context.SaveChangesAsync();
            }

            return Ok(ticket.QRCodeSvg);
        }
    }
}
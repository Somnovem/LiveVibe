using LiveVibe.Server.HelperClasses.Extensions;
using LiveVibe.Server.Models.DTOs.ModelDTOs;
using LiveVibe.Server.Models.DTOs.Requests.EventSeatTypes;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/event-seat-types")]
    public class EventSeatTypeController : Controller
    {
        private readonly ApplicationContext _context;

        public EventSeatTypeController(ApplicationContext db)
        {
            _context = db;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Admin] Retrieve all event seat types.", Description = "Returns a list of all event seat types in the database.")]
        [SwaggerResponse(200, "Success", typeof(PagedListDTO<EventSeatType>))]
        public async Task<ActionResult<PagedListDTO<EventSeatType>>> GetEventSeatTypes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var seatTypes = await _context.EventSeatTypes
                .AsNoTracking()
                .ToPagedListAsync(pageNumber, pageSize);

            return Ok(seatTypes.ToDto());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Get event seat type by ID.")]
        [SwaggerResponse(200, "Event seat type found.", typeof(EventSeatType))]
        [SwaggerResponse(404, "Event seat type not found.")]
        public async Task<ActionResult<EventSeatType>> GetEventSeatTypeById(Guid id)
        {
            var eventSeatType = await _context.EventSeatTypes.FindAsync(id);
            if (eventSeatType == null)
                return NotFound("Event seat type not found.");

            return Ok(eventSeatType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "[Admin] Create a new event seat type.", Description = "If provided with valid data, create a new event seat type and generate corresponding tickets.")]
        [SwaggerResponse(201, "Event seat type created successfully.", typeof(EventSeatType))]
        [SwaggerResponse(400, "Invalid input.")]
        [SwaggerResponse(409, "Same event seat type already exists")]
        public async Task<ActionResult<EventSeatType>> CreateEventSeatType([FromBody] CreateEventSeatTypeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool eventSeatTypeExists = await _context.EventSeatTypes.AnyAsync(e => e.EventId == request.EventId
                                                                    && e.Name == request.Name.Trim());
            if (eventSeatTypeExists)
            {
                return Conflict("This event seat type already exists.");
            }

            var eventSeatType = new EventSeatType
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                Name = request.Name.Trim(),
                Capacity = request.Capacity,
                AvailableSeats = request.AvailableSeats,
                Price = request.Price
            };

            _context.EventSeatTypes.Add(eventSeatType);

            for (int i = 1; i <= request.Capacity; i++)
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    EventId = request.EventId,
                    SeatingCategoryId = eventSeatType.Id,
                    Seat = $"{eventSeatType.Name}-{i:D3}"
                };

                _context.Tickets.Add(ticket);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventSeatTypeById),
                new { id = eventSeatType.Id },
                eventSeatType
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [SwaggerOperation(Summary = "[Admin] Update an existing event seat type.", Description = "Updates existing event seat type details.")]
        [SwaggerResponse(200, "Event seat type updated successfully", typeof(EventSeatType))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Event seat type not found")]
        [SwaggerResponse(409, "Same event seat type already exists")]
        public async Task<ActionResult<EventSeatType>> UpdateEventSeatType([FromBody] UpdateEventSeatTypeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eventSeatType = await _context.EventSeatTypes.FirstOrDefaultAsync(e => e.Id == request.Id);
            if (eventSeatType == null)
                return NotFound($"No event seat type found with ID {request.Id}");

            bool eventSeatTypeExists = await _context.EventSeatTypes.AnyAsync(e => e.EventId == request.EventId
                                                                    && e.Name == request.Name.Trim()
                                                                    && e.Id != request.Id);
            if (eventSeatTypeExists)
            {
                return Conflict("This event seat type already exists.");
            }

            eventSeatType.EventId = request.EventId;
            eventSeatType.Name = request.Name.Trim();
            eventSeatType.Capacity = request.Capacity;
            eventSeatType.AvailableSeats = request.AvailableSeats;
            eventSeatType.Price = request.Price;

            _context.EventSeatTypes.Update(eventSeatType);
            await _context.SaveChangesAsync();

            return Ok(eventSeatType);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Delete an event seat type.", Description = "Deletes the event seat type with the specified ID.")]
        [SwaggerResponse(200, "Event seat type deleted successfully")]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Event seat type not found")]
        public async Task<IActionResult> DeleteEventSeatType(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eventSeatType = await _context.EventSeatTypes.FirstOrDefaultAsync(e => e.Id == id);
            if (eventSeatType == null)
                return NotFound("Event seat type not found.");

            _context.EventSeatTypes.Remove(eventSeatType);
            await _context.SaveChangesAsync();

            return Ok("Event seat type deleted successfully.");
        }
    }
}

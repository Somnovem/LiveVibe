using LiveVibe.Server.Models.DTOs.Requests.EventCategories;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/event-categories")]
    public class EventCategoryController : Controller
    {

        private readonly ApplicationContext _context;

        public EventCategoryController(ApplicationContext db)
        {
            _context = db;
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Any] Retrieve all event categories", Description = "Returns a list of all event categories in the database.")]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<string>))]
        public async Task<IEnumerable<string>> GetEventCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalEventCategories = await _context.EventCategories.CountAsync();
            var totalPages = (int)Math.Ceiling(totalEventCategories / (double)pageSize);

            Response.Headers.Append("X-Total-Count", totalEventCategories.ToString());
            Response.Headers.Append("X-Total-Pages", totalPages.ToString());

            return await _context.EventCategories.AsNoTracking()
                                                    .Select(e => e.Name)
                                                    .Skip((pageNumber - 1) * pageSize)
                                                    .Take(pageSize)
                                                    .ToListAsync();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Get event category by ID")]
        [SwaggerResponse(200, "Event category found", typeof(EventCategory))]
        [SwaggerResponse(404, "Event category not found")]
        public async Task<ActionResult<EventCategory>> GetEventCategoryById(Guid id)
        {
            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "[Admin] Create a new event category", Description = "If provided with valid data, create a new event category")]
        [SwaggerResponse(201, "Event category created successfully", typeof(EventCategory))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(409, "Event category with the same name already exists")]
        public async Task<ActionResult<EventCategory>> CreateEventCategory([FromBody] CreateEventCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool nameExists = await _context.EventCategories.AnyAsync(u => u.Name == request.Name);
            if (nameExists)
            {
                return Conflict("An event category with this name already exists.");
            }

            var eventCategory = new EventCategory
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            _context.EventCategories.Add(eventCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventCategoryById),
                new { id = eventCategory.Id },
                eventCategory
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [SwaggerOperation(Summary = "[Admin] Update an existing event category", Description = "Updates existing event category.")]
        [SwaggerResponse(200, "Event category updated successfully", typeof(EventCategory))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Event category not found")]
        public async Task<ActionResult<EventCategory>> UpdateEventCategory([FromBody] UpdateEventCategoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eventCategory = await _context.EventCategories.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (eventCategory == null)
                return NotFound($"No event category found with ID {request.Id}");

            eventCategory.Name = request.Name;

            _context.EventCategories.Update(eventCategory);
            await _context.SaveChangesAsync();

            return Ok(eventCategory);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Delete an event category", Description = "Deletes the event category with the specified ID.")]
        [SwaggerResponse(200, "Event category deleted successfully")]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Event category not found")]
        public async Task<IActionResult> DeleteEventCategory(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eventCategory = await _context.EventCategories.FirstOrDefaultAsync(u => u.Id == id);
            if (eventCategory == null)
                return NotFound("Event category not found.");

            _context.EventCategories.Remove(eventCategory);
            await _context.SaveChangesAsync();

            return Ok("Event category deleted successfully.");
        }
    }
}

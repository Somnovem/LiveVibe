using LiveVibe.Server.HelperClasses.Extensions;
using LiveVibe.Server.Models.DTOs.ModelDTOs;
using LiveVibe.Server.Models.DTOs.Requests.Organizers;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/organizers")]
    public class OrganizerController : Controller
    {

        private readonly ApplicationContext _context;

        public OrganizerController(ApplicationContext db)
        {
            _context = db;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Admin] Retrieve all organizers.", Description = "Returns a list of all organizers in the database.")]
        [SwaggerResponse(200, "Success", typeof(PagedListDTO<Organizer>))]
        public async Task<ActionResult<PagedListDTO<Organizer>>> GetOrganizers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var organizers = await _context.Organizers
                                            .AsNoTracking()
                                            .ToPagedListAsync(pageNumber, pageSize);
            
            return Ok(organizers.ToDto());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "[Admin] Get organizer by ID.")]
        [SwaggerResponse(200, "Organizer found.", typeof(Organizer))]
        [SwaggerResponse(404, "Organizer not found.")]
        public async Task<ActionResult<Organizer>> GetOrganizerById(Guid id)
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
                return NotFound();

            return Ok(organizer);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "[Admin] Create a new organizer.", Description = "If provided with valid data, create a new organizer.")]
        [SwaggerResponse(201, "Organizer created successfully.", typeof(Organizer))]
        [SwaggerResponse(400, "Invalid input.")]
        [SwaggerResponse(409, "Organizer with the same email already exists.")]
        public async Task<ActionResult<Organizer>> CreateOrganizer([FromBody] CreateOrganizerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool emailExists = await _context.Organizers.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
            {
                return Conflict("An organizer with this email already exists.");
            }

            var organizer = new Organizer
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOrganizerById),
                new { id = organizer.Id },
                organizer
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [SwaggerOperation(Summary = "[Admin] Update an existing organizer.", Description = "Updates existing organizer details.")]
        [SwaggerResponse(200, "Organizer updated successfully.", typeof(Organizer))]
        [SwaggerResponse(400, "Invalid input.")]
        [SwaggerResponse(404, "Organizer not found.")]
        public async Task<ActionResult<Organizer>> UpdateOrganizer([FromBody] UpdateOrganizerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organizer = await _context.Organizers.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (organizer == null)
                return NotFound($"No organizer found with ID {request.Id}");

            organizer.Name = request.Name;
            organizer.Phone = request.Phone;
            organizer.Email = request.Email;
            organizer.UpdatedAt = DateTime.UtcNow;

            _context.Organizers.Update(organizer);
            await _context.SaveChangesAsync();

            return Ok(organizer);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        [SwaggerOperation(Summary = "[Admin] Delete an organizer.", Description = "Deletes the organizer with the specified ID.")]
        [SwaggerResponse(200, "Organizer deleted successfully.")]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Organizer not found.")]
        public async Task<IActionResult> DeleteOrganizer(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organizer = await _context.Organizers.FirstOrDefaultAsync(u => u.Id == id);
            if (organizer == null)
                return NotFound("Organizer not found.");

            _context.Organizers.Remove(organizer);
            await _context.SaveChangesAsync();

            return Ok("Organizer deleted successfully.");
        }
    }
}

using LiveVibe.Server.Models.DTOs.Requests.Countries;
using LiveVibe.Server.Models.DTOs.Responses;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/cities")]
    public class CityController : Controller
    {

        private readonly ApplicationContext _context;

        public CityController(ApplicationContext db)
        {
            _context = db;
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Any] Retrieve all cities", Description = "Returns a list of all cities in the database.")]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<string>))]
        public async Task<IEnumerable<string>> GetCities()
        { 
            return await _context.Cities.AsNoTracking()
                                              .Select(e => e.Name)
                                              .ToListAsync();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Get city by ID")]
        [SwaggerResponse(200, "City found", typeof(City))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "City not found", typeof(ErrorDTO))]
        public async Task<ActionResult<City>> GetCityById(Guid id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound(new ErrorDTO("City not found"));

            return Ok(city);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "[Admin] Create a new city", Description = "If provided with valid data, create a new city.")]
        [SwaggerResponse(201, "City created successfully", typeof(City))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(409, "City with the same name already exists", typeof(ErrorDTO))]
        public async Task<ActionResult<City>> CreateCity([FromBody] CreateCityRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool nameExists = await _context.Cities.AnyAsync(u => u.Name == request.Name);
            if (nameExists)
            {
                return Conflict(new ErrorDTO("A city with this name already exists."));
            }

            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCityById),
                new { id = city.Id },
                city
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [SwaggerOperation(Summary = "[Admin] Update an existing city", Description = "Updates existing city details.")]
        [SwaggerResponse(200, "City updated successfully", typeof(City))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "City not found", typeof(ErrorDTO))]
        public async Task<ActionResult<City>> UpdateCity([FromBody] UpdateCityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = await _context.Cities.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (city == null)
                return NotFound(new ErrorDTO($"No city found with ID {request.Id}"));

            city.Name = request.Name;

            _context.Cities.Update(city);
            await _context.SaveChangesAsync();

            return Ok(city);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Delete a city", Description = "Deletes the city with the specified ID.")]
        [SwaggerResponse(200, "City deleted successfully", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "City not found", typeof(ErrorDTO))]
        public async Task<IActionResult> DeleteCity(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = await _context.Cities.FirstOrDefaultAsync(u => u.Id == id);
            if (city == null)
                return NotFound(new ErrorDTO("City not found."));

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return Ok(new SuccessDTO("City deleted successfully."));
        }
    }
}

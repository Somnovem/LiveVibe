using LiveVibe.Server.Models.DTOs.Requests.Countries;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/countries")]
    public class CountryController : Controller
    {

        private readonly ApplicationContext _context;

        public CountryController(ApplicationContext db)
        {
            _context = db;
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Any] Retrieve all countries", Description = "Returns a list of all countries in the database.")]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<string>))]
        public async Task<IEnumerable<string>> GetCountries(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalCountries = await _context.Countries.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCountries / (double)pageSize);

            Response.Headers.Append("X-Total-Count", totalCountries.ToString());
            Response.Headers.Append("X-Total-Pages", totalPages.ToString());

            return await _context.Countries.AsNoTracking()
                                              .Select(e => e.Name)
                                              .Skip((pageNumber - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Get country by ID")]
        [SwaggerResponse(200, "Country found", typeof(Country))]
        [SwaggerResponse(404, "Country not found")]
        public async Task<ActionResult<Country>> GetCountryById(Guid id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
                return NotFound();

            return Ok(country);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "[Admin] Create a new country", Description = "If provided with valid data, create a new country.")]
        [SwaggerResponse(201, "Country created successfully", typeof(Country))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(409, "Country with the same name already exists")]
        public async Task<ActionResult<Country>> CreateCountry([FromBody] CreateCountryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool nameExists = await _context.Countries.AnyAsync(u => u.Name == request.Name);
            if (nameExists)
            {
                return Conflict("A country with this name already exists.");
            }

            var country = new Country
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCountryById),
                new { id = country.Id },
                country
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [SwaggerOperation(Summary = "[Admin] Update an existing country", Description = "Updates existing country details.")]
        [SwaggerResponse(200, "Country updated successfully", typeof(Country))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Country not found")]
        public async Task<ActionResult<Country>> UpdateCountry([FromBody] UpdateCountryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var country = await _context.Countries.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (country == null)
                return NotFound($"No country found with ID {request.Id}");

            country.Name = request.Name;

            _context.Countries.Update(country);
            await _context.SaveChangesAsync();

            return Ok(country);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Delete a country", Description = "Deletes the country with the specified ID.")]
        [SwaggerResponse(200, "Country deleted successfully")]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Country not found")]
        public async Task<IActionResult> DeleteCountry(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var country = await _context.Countries.FirstOrDefaultAsync(u => u.Id == id);
            if (country == null)
                return NotFound("Country not found.");

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return Ok("Country deleted successfully.");
        }
    }
}

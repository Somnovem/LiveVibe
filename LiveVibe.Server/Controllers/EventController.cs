using LiveVibe.Server.Models.DTOs.Requests.Events;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using SixLabors.ImageSharp;
using LiveVibe.Server.Models.DTOs.ModelDTOs;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/events")]
    public class EventController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationContext _context;

        public EventController(ApplicationContext db, IWebHostEnvironment env)
        {
            _context = db;
            _env = env;
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Any] Retrieve all events with pagination.", Description = "Returns a paged list of events.")]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<Event>))]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var events = await _context.Events
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalEvents = await _context.Events.CountAsync();
            var totalPages = (int)Math.Ceiling(totalEvents / (double)pageSize);

            Response.Headers.Append("X-Total-Count", totalEvents.ToString());
            Response.Headers.Append("X-Total-Pages", totalPages.ToString());

            return Ok(events);
        }


        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "[Any] Get event by ID.")]
        [SwaggerResponse(200, "Event found.", typeof(EventDTO))]
        [SwaggerResponse(404, "Event not found.")]
        public async Task<ActionResult<EventDTO>> GetEventById(Guid id)
        {
            var _event = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.EventCategory)
                .Include(e => e.City)
                .Include(e => e.EventSeatTypes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (_event == null)
                return NotFound();

            var result = new EventDTO
            {
                Id = _event.Id,
                Title = _event.Title,
                Description = _event.Description,
                Organizer = _event.Organizer.Name,
                Category = _event.EventCategory.Name,
                City = _event.City.Name,
                Location = _event.Location,
                Time = _event.Time,
                ImageUrl = _event.ImageUrl,
                CreatedAt = _event.CreatedAt,
                UpdatedAt = _event.UpdatedAt,
                SeatTypes = _event.EventSeatTypes
                    .Select(sc => new ShortSeatTypeDTO(sc))
                    .ToList()
            };

            return Ok(result);
        }


        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "[Any] Search for events with filtering and pagination.",
            Description = "Search events based on filters like title, category, city, and date range. Supports pagination via PageNumber and PageSize."
        )]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<ShortEventDTO>))]
        [SwaggerResponse(400, "Invalid request")]
        public async Task<ActionResult<IEnumerable<ShortEventDTO>>> Search([FromQuery] EventSearchRequest searchRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Organizer)
                .Include(e => e.EventCategory)
                .Include(e => e.City)
                .Include(e => e.EventSeatTypes)
                .Where(e => e.Time >= DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(searchRequest.Title))
                query = query.Where(e => e.Title.Contains(searchRequest.Title.Trim()));

            if (!string.IsNullOrWhiteSpace(searchRequest.Category))
                query = query.Where(e => e.EventCategory.Name == searchRequest.Category.Trim());

            if (!string.IsNullOrWhiteSpace(searchRequest.City))
                query = query.Where(e => e.City.Name == searchRequest.City.Trim());

            if (searchRequest.DateFrom.HasValue)
                query = query.Where(e => e.Time >= searchRequest.DateFrom.Value);

            if (searchRequest.DateTo.HasValue)
                query = query.Where(e => e.Time <= searchRequest.DateTo.Value);

            if (searchRequest.MinPrice.HasValue || searchRequest.MaxPrice.HasValue)
            {
                query = query.Where(e =>
                    e.EventSeatTypes.Any(sc =>
                        (!searchRequest.MinPrice.HasValue || (decimal)sc.Price >= searchRequest.MinPrice.Value) &&
                        (!searchRequest.MaxPrice.HasValue || (decimal)sc.Price <= searchRequest.MaxPrice.Value)
                    )
                );
            }

            var skip = (searchRequest.PageNumber - 1) * searchRequest.PageSize;
            var take = searchRequest.PageSize;

            var rawResults = await query 
                                    .OrderBy(e => e.Time)
                                    .Skip(skip)
                                    .Take(take)
                                    .ToListAsync();

            var results = rawResults.Select(e =>
            {
                var matchedCategory = e.EventSeatTypes
                    .FirstOrDefault(sc =>
                        (!searchRequest.MinPrice.HasValue || (decimal)sc.Price >= searchRequest.MinPrice.Value) &&
                        (!searchRequest.MaxPrice.HasValue || (decimal)sc.Price <= searchRequest.MaxPrice.Value)
                    );

                return new ShortEventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = !string.IsNullOrEmpty(e.Description)
                        ? (e.Description.Length > 50 ? e.Description.Substring(0, 50) + "..." : e.Description)
                        : null,
                    Organizer = e.Organizer.Name,
                    Category = e.EventCategory.Name,
                    City = e.City.Name,
                    Location = e.Location,
                    Time = e.Time,
                    ImageUrl = e.ImageUrl,
                    MatchedSeatCategoryName = matchedCategory?.Name,
                    MatchedSeatCategoryPrice = matchedCategory?.Price != null ? (decimal)matchedCategory.Price : null,
                };
            }).ToList();

            return Ok(results);
        }

        [HttpGet("{eventId:guid}/seat-types")]
        [SwaggerOperation(Summary = "[Any] Get seat types for a specific event", Description = "Returns the names of all seat types for a given event ID.")]
        [SwaggerResponse(200, "Seat types retrieved successfully", typeof(IEnumerable<string>))]
        [SwaggerResponse(404, "Event not found")]
        public async Task<ActionResult<IEnumerable<string>>> GetSeatTypesForEvent(Guid eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
                return NotFound("Event not found.");

            var seatTypeNames = await _context.EventSeatTypes
                .Where(e => e.EventId == eventId)
                .Select(e => new ShortSeatTypeDTO(e))
                .ToListAsync();

            return Ok(seatTypeNames);
        }

        [HttpGet("{eventId:guid}/seat-types/{seatTypeId:guid}/available-tickets")]
        [SwaggerOperation(Summary = "[Any] Get available tickets for a specific seat type in an event")]
        [SwaggerResponse(200, "Available tickets retrieved successfully", typeof(IEnumerable<TicketDTO>))]
        [SwaggerResponse(404, "Event or Seat Type not found")]
        public async Task<ActionResult<IEnumerable<TicketDTO>>> GetAvailableTickets(Guid eventId, Guid seatTypeId)
        {
            var seatTypeExists = await _context.EventSeatTypes.AnyAsync(s =>
                s.EventId == eventId && s.Id == seatTypeId);

            if (!seatTypeExists)
                return NotFound("Seat type not found for this event.");

            var availableTickets = await _context.Tickets
                .Where(t => t.EventId == eventId && t.SeatingCategoryId == seatTypeId)
                .Where(t => !_context.TicketPurchases.Any(p => p.TicketId == t.Id && !p.WasRefunded))
                .Select(t => new ShortTicketDTO(t))
                .ToListAsync();

            return Ok(availableTickets);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "[Admin] Create a new event.", Description = "If provided with valid data, create a new event.")]
        [SwaggerResponse(201, "Event created successfully.", typeof(Event))]
        [SwaggerResponse(400, "Invalid input.")]
        [SwaggerResponse(409, "Same event already exists")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool organizerExists = await _context.Organizers.AnyAsync(o => o.Id == request.OrganizerId);
            if (!organizerExists)
            {
                return BadRequest("Organizer with this Id doesn't exist.");
            }

            bool categoryExists = await _context.EventCategories.AnyAsync(e => e.Id == request.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Event Category with this Id doesn't exist.");
            }

            bool countryExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId);
            if (!countryExists)
            {
                return BadRequest("Country with this Id doesn't exist.");
            }

            bool eventExists = await _context.Events.AnyAsync(e => e.OrganizerId == request.OrganizerId
                                                    && e.CategoryId == request.CategoryId
                                                    && e.CityId == request.CityId
                                                    && e.Location == request.Location.Trim()
                                                    && e.Time == request.Time
                                                    && e.Title == request.Title.Trim());
            if (eventExists)
            {
                return Conflict("This event already exists.");
            }

            var _event = new Event
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                OrganizerId = request.OrganizerId,
                CategoryId = request.CategoryId,
                Location = request.Location.Trim(),
                CityId = request.CityId,
                Time = request.Time,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(_event);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = _event.Id },
                _event
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [SwaggerOperation(Summary = "[Admin] Update an existing event.", Description = "Updates existing event details.")]
        [SwaggerResponse(200, "Event updated successfully", typeof(Event))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Event not found")]
        [SwaggerResponse(409, "Same event already exists")]
        public async Task<ActionResult<Event>> UpdateEvent([FromBody] UpdateEventRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var _event = await _context.Events.FirstOrDefaultAsync(e => e.Id == request.Id);
            if (_event == null)
                return NotFound($"No event found with ID {request.Id}");

            bool organizerExists = await _context.Organizers.AnyAsync(o => o.Id == request.OrganizerId);
            if (!organizerExists)
            {
                return BadRequest("Organizer with this Id doesn't exist.");
            }

            bool categoryExists = await _context.EventCategories.AnyAsync(e => e.Id == request.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Event Category with this Id doesn't exist.");
            }

            bool countryExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId);
            if (!countryExists)
            {
                return BadRequest("Country with this Id doesn't exist.");
            }

            bool eventExists = await _context.Events.AnyAsync(e => e.OrganizerId == request.OrganizerId
                                                    && e.CategoryId == request.CategoryId
                                                    && e.CityId == request.CityId
                                                    && e.Location == request.Location.Trim()
                                                    && e.Time == request.Time
                                                    && e.Title == request.Title.Trim()
                                                    && e.Description == request.Description.Trim());
            if (eventExists)
            {
                return Conflict("This event already exists.");
            }

            _event.Title = request.Title.Trim();
            _event.Description = request.Description.Trim();
            _event.OrganizerId = request.OrganizerId;
            _event.CategoryId = request.CategoryId;
            _event.Location = request.Location.Trim();
            _event.CityId = request.CityId;
            _event.Time = request.Time;
            _event.UpdatedAt = DateTime.UtcNow;

            _context.Events.Update(_event);
            await _context.SaveChangesAsync();

            return Ok(_event);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Delete an event.", Description = "Deletes the event with the specified ID.")]
        [SwaggerResponse(200, "Event deleted successfully")]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Event not found")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var _event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (_event == null)
                return NotFound("Event not found.");

            _context.Events.Remove(_event);
            await _context.SaveChangesAsync();

            return Ok("Event deleted successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload-photo/{eventId:guid}")]
        [SwaggerOperation(
            Summary = "[Admin] Upload a photo for an event.",
            Description = "Allows an Admin user to upload an image file for a specific event by its ID. " +
                          "The image must be one of the supported formats (.jpg, .jpeg, .png, .gif, .webp) and up to 5MB."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Photo uploaded successfully.", typeof(object))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid file or request (e.g., no file, unsupported format, file too large, invalid image).")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Event not found.")]
        public async Task<IActionResult> UploadEventPhoto(Guid eventId, IFormFile file)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null)
                return NotFound("Event not found.");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return BadRequest("File size exceeds 5MB limit.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Unsupported file extension.");

            if (!allowedMimeTypes.Contains(file.ContentType))
                return BadRequest("Unsupported MIME type.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            fileName = Path.GetFileName(fileName);

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "events");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            //double-check it's a valid image
            try
            {
                using var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream());
            }
            catch
            {
                return BadRequest("Uploaded file is not a valid image.");
            }

            // Delete existing image if present
            if (!string.IsNullOrEmpty(eventItem.ImageUrl))
            {
                var existingImagePath = Path.Combine(_env.WebRootPath, eventItem.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(existingImagePath))
                {
                    System.IO.File.Delete(existingImagePath);
                }
            }

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            eventItem.ImageUrl = $"/images/events/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl = eventItem.ImageUrl });
        }

        [HttpGet("featured")]
        [SwaggerOperation(Summary = "[Any] Get featured events for the main page.", 
            Description = "Returns a curated list of upcoming events based on time proximity, ticket availability, and category diversity.")]
        [SwaggerResponse(200, "Success", typeof(IEnumerable<FeaturedEventDTO>))]
        public async Task<ActionResult<IEnumerable<FeaturedEventDTO>>> GetFeaturedEvents(
            [FromQuery] int maxEvents = 6)
        {
            var now = DateTime.UtcNow;
            var nearFuture = now.AddDays(30);

            var upcomingEvents = await _context.Events
                .AsNoTracking()
                .Include(e => e.Organizer)
                .Include(e => e.EventCategory)
                .Include(e => e.City)
                .Include(e => e.EventSeatTypes)
                .ThenInclude(st => st.Tickets)
                .Where(e => e.Time >= now && e.Time <= nearFuture) // Only events within 30 days
                .Where(e => e.EventSeatTypes.Any(st => 
                    st.Tickets.Any(t => !_context.TicketPurchases
                        .Any(tp => tp.TicketId == t.Id && !tp.WasRefunded)))) // Has available tickets
                .ToListAsync();

            var eventsByCategory = upcomingEvents // Group by category to ensure diversity
                .GroupBy(e => e.EventCategory.Name)
                .ToDictionary(g => g.Key, g => g.ToList());

            var featuredEvents = new List<Event>();
            var categories = eventsByCategory.Keys.ToList();
            var categoryIndex = 0;
            
            while (featuredEvents.Count < maxEvents && categories.Any()) // Ensure we get events from different categories in rotation
            {
                var currentCategory = categories[categoryIndex % categories.Count];
                var categoryEvents = eventsByCategory[currentCategory];

                if (categoryEvents.Any())
                {
                    var bestEvent = categoryEvents
                        .OrderBy(e => e.Time)
                        .FirstOrDefault();

                    if (bestEvent != null)
                    {
                        featuredEvents.Add(bestEvent);
                        categoryEvents.Remove(bestEvent);
                    }

                    if (!categoryEvents.Any())
                    {
                        categories.Remove(currentCategory);
                    }
                }

                categoryIndex++;
            }

            var result = featuredEvents.Select(e => new FeaturedEventDTO(e)).ToList();

            return Ok(result);
        }
    }
}

using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.ModelDTOs;
using LiveVibe.Server.Models.DTOs.Responses;
using LiveVibe.Server.Models.DTOs.Requests.Events;
using LiveVibe.Server.Models.DTOs.Shared;
using LiveVibe.Server.HelperClasses.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using SixLabors.ImageSharp;
using LiveVibe.Server.HelperClasses.Collections;

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
        [SwaggerResponse(200, "Success", typeof(PagedListDTO<EventDTO>))]
        public async Task<ActionResult<PagedListDTO<EventDTO>>> GetEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.Organizer)
                .Include(e => e.EventCategory)
                .Include(e => e.City)
                .Include(e => e.EventSeatTypes)
                .ToPagedListAsync(pageNumber, pageSize);

            var eventDtos = events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Organizer = e.Organizer.Name,
                Category = e.EventCategory.Name,
                City = e.City.Name,
                Location = e.Location,
                Time = e.Time,
                ImageUrl = e.ImageUrl,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                SeatTypes = e.EventSeatTypes
                    .Select(sc => new ShortSeatTypeDTO(sc))
                    .ToList()
            }).ToList();

            var result = new PagedListDTO<EventDTO>
            {
                Items = eventDtos,
                Page = events.Page,
                TotalPages = events.TotalPages,
                PageSize = events.PageSize,
                TotalCount = events.TotalCount,
                HasPrevious = events.HasPrevious,
                HasNext = events.HasNext
            };

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "[Any] Get event by ID.")]
        [SwaggerResponse(200, "Event found.", typeof(EventDTO))]
        [SwaggerResponse(404, "Event not found.", typeof(ErrorDTO))]
        public async Task<ActionResult<EventDTO>> GetEventById(Guid id)
        {
            var _event = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.EventCategory)
                .Include(e => e.City)
                .Include(e => e.EventSeatTypes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (_event == null)
                return NotFound(new ErrorDTO("Event not found."));

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
            Description = "Search events based on filters like title, price, category, city, and date range. Supports pagination via PageNumber and PageSize."
        )]
        [SwaggerResponse(200, "Success", typeof(PagedListDTO<SearchedEventDTO>))]
        [SwaggerResponse(400, "Invalid request")]
        public async Task<ActionResult<PagedListDTO<SearchedEventDTO>>> Search([FromQuery] EventSearchRequest searchRequest)
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

            if (searchRequest.OrderByTime)
                query = query.OrderBy(e => e.Time);

            var rawResults = await query 
                                    .OrderBy(e => e.Time)
                                    .ToPagedListAsync(searchRequest.PageNumber, searchRequest.PageSize);

            var mappedResults = rawResults.Select(e =>
            {
                var matchedCategory = e.EventSeatTypes
                    .FirstOrDefault(sc =>
                        (!searchRequest.MinPrice.HasValue || (decimal)sc.Price >= searchRequest.MinPrice.Value) &&
                        (!searchRequest.MaxPrice.HasValue || (decimal)sc.Price <= searchRequest.MaxPrice.Value)
                    );

                return new SearchedEventDTO
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

            var pagedMapped = new PagedList<SearchedEventDTO>(
                mappedResults,
                rawResults.TotalCount,
                rawResults.Page,
                rawResults.PageSize
            );

            return Ok(pagedMapped.ToDto());
        }

        [HttpGet("{eventId:guid}/seat-types")]
        [SwaggerOperation(Summary = "[Any] Get seat types for a specific event", Description = "Returns the names of all seat types for a given event ID.")]
        [SwaggerResponse(200, "Seat types retrieved successfully", typeof(IEnumerable<string>))]
        [SwaggerResponse(404, "Event not found", typeof(ErrorDTO))]
        public async Task<ActionResult<IEnumerable<string>>> GetSeatTypesForEvent(Guid eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
                return NotFound(new ErrorDTO("Event not found."));

            var seatTypeNames = await _context.EventSeatTypes
                .Where(e => e.EventId == eventId)
                .Select(e => new ShortSeatTypeDTO(e))
                .ToListAsync();

            return Ok(seatTypeNames);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "[Admin] Create a new event.", Description = "If provided with valid data, create a new event.")]
        [SwaggerResponse(201, "Event created successfully.", typeof(Event))]
        [SwaggerResponse(400, "Invalid input.", typeof(ErrorDTO))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "Organizer, Event Category or City with provided ID doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(409, "Same event already exists", typeof(ErrorDTO))]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool organizerExists = await _context.Organizers.AnyAsync(o => o.Id == request.OrganizerId);
            if (!organizerExists)
            {
                return NotFound(new ErrorDTO("Organizer with this Id doesn't exist."));
            }

            bool categoryExists = await _context.EventCategories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return NotFound(new ErrorDTO("Event Category with this Id doesn't exist."));
            }

            bool cityExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId);
            if (!cityExists)
            {
                return NotFound(new ErrorDTO("City with this Id doesn't exist."));
            }

            bool eventExists = await _context.Events.AnyAsync(e => e.OrganizerId == request.OrganizerId
                                                    && e.CategoryId == request.CategoryId
                                                    && e.CityId == request.CityId
                                                    && e.Location == request.Location.Trim()
                                                    && e.Time == request.Time
                                                    && e.Title == request.Title.Trim());
            if (eventExists)
            {
                return Conflict(new ErrorDTO("This event already exists."));
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
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "Event not found", typeof(ErrorDTO))]
        [SwaggerResponse(409, "Same event already exists", typeof(ErrorDTO))]
        public async Task<ActionResult<Event>> UpdateEvent([FromBody] UpdateEventRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var _event = await _context.Events.FirstOrDefaultAsync(e => e.Id == request.Id);
            if (_event == null)
                return NotFound(new ErrorDTO("Event with this Id doesn't exist."));

            bool organizerExists = await _context.Organizers.AnyAsync(o => o.Id == request.OrganizerId);
            if (!organizerExists)
            {
                return NotFound(new ErrorDTO("Organizer with this Id doesn't exist."));
            }

            bool categoryExists = await _context.EventCategories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return NotFound(new ErrorDTO("Event Category with this Id doesn't exist."));
            }

            bool cityExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId);
            if (!cityExists)
            {
                return NotFound(new ErrorDTO("City with this Id doesn't exist."));
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
                return Conflict(new ErrorDTO("This event already exists."));
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
        [SwaggerResponse(200, "Event deleted successfully", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "Event not found", typeof(ErrorDTO))]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var _event = await _context.Events
                .Include(e => e.EventSeatTypes)
                    .ThenInclude(st => st.Tickets)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (_event == null)
                return NotFound(new ErrorDTO("Event not found."));

            // Delete all related tickets (with refund placeholder)
            foreach (var seatType in _event.EventSeatTypes)
            {
                foreach (var ticket in seatType.Tickets.ToList())
                {
                    // Placeholder: If the event hasn't happened yet, do refund logic here
                    if (_event.Time > DateTime.UtcNow)
                    {
                        // TODO: Implement refund logic for ticket
                    }
                    _context.Tickets.Remove(ticket);
                }
            }

            // Delete all related seat types
            foreach (var seatType in _event.EventSeatTypes.ToList())
            {
                _context.EventSeatTypes.Remove(seatType);
            }

            // Delete the event
            _context.Events.Remove(_event);
            await _context.SaveChangesAsync();

            // Remove any now-empty orders after deleting the event and its tickets
            var emptyOrders = await _context.Orders
                .Include(o => o.Tickets)
                .Where(o => !o.Tickets.Any())
                .ToListAsync();

            if (emptyOrders.Any())
            {
                _context.Orders.RemoveRange(emptyOrders);
                await _context.SaveChangesAsync();
            }

            return Ok(new SuccessDTO("Event deleted successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload-photo/{eventId:guid}")]
        [SwaggerOperation(
            Summary = "[Admin] Upload a photo for an event.",
            Description = "Allows an Admin user to upload an image file for a specific event by its ID. " +
                          "The image must be one of the supported formats (.jpg, .jpeg, .png, .gif, .webp) and up to 5MB."
        )]
        [SwaggerResponse(200, "Photo uploaded successfully.", typeof(UploadPhotoSuccessDTO))]
        [SwaggerResponse(400, "Invalid file or request (e.g., no file, unsupported format, file too large, invalid image).",typeof(ErrorDTO))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "Event not found.", typeof(ErrorDTO))]
        public async Task<IActionResult> UploadEventPhoto(Guid eventId, IFormFile file)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null)
                return NotFound(new ErrorDTO("Event not found."));

            if (file == null || file.Length == 0)
                return BadRequest(new ErrorDTO("No file uploaded."));

            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return BadRequest(new ErrorDTO("File size exceeds 5MB limit."));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new ErrorDTO("Unsupported file extension."));

            if (!allowedMimeTypes.Contains(file.ContentType))
                return BadRequest(new ErrorDTO("Unsupported MIME type."));

            var fileName = $"{Guid.NewGuid()}{extension}";
            fileName = Path.GetFileName(fileName);

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "events");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            //double-check it's a valid image
            try
            {
                using var image = Image.Load(file.OpenReadStream());
            }
            catch
            {
                return BadRequest(new ErrorDTO("Uploaded file is not a valid image."));
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

            return Ok(new UploadPhotoSuccessDTO(eventItem.ImageUrl));
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
                .Where(e => e.Time >= now && e.Time <= nearFuture)
                .Where(e => e.EventSeatTypes.Any(st => st.AvailableSeats > 0))
                .ToListAsync();

            var eventsByCategory = upcomingEvents
                .GroupBy(e => e.EventCategory.Name)
                .ToDictionary(g => g.Key, g => g.ToList());

            var featuredEvents = new List<Event>();
            var categories = eventsByCategory.Keys.ToList();
            var categoryIndex = 0;

            while (featuredEvents.Count < maxEvents && categories.Any())
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

            return Ok(featuredEvents.Select(e => new FeaturedEventDTO(e)));
        }
    }
}

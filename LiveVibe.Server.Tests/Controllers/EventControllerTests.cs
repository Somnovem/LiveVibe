using FluentAssertions;
using LiveVibe.Server.Controllers;
using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.ModelDTOs;
using LiveVibe.Server.Models.DTOs.Requests.Events;
using LiveVibe.Server.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiveVibe.Server.Tests.Controllers;

public class EventControllerTests : TestBase
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public EventControllerTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(x => x.WebRootPath).Returns("wwwroot");
    }

    private async Task<(Event, EventCategory, City, Organizer)> SetupTestData(ApplicationContext context, string suffix = "")
    {
        var category = new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = $"Test Category {suffix}"
        };

        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = $"Test City {suffix}"
        };

        var organizer = new Organizer
        {
            Id = Guid.NewGuid(),
            Name = $"Test Organizer {suffix}",
            Phone = "1234567890",
            Email = $"test{suffix}@organizer.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.EventCategories.AddAsync(category);
        await context.Cities.AddAsync(city);
        await context.Organizers.AddAsync(organizer);
        await context.SaveChangesAsync();

        var testEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = $"Test Event {suffix}",
            Description = $"Test Description {suffix}",
            OrganizerId = organizer.Id,
            CategoryId = category.Id,
            Location = $"Test Location {suffix}",
            CityId = city.Id,
            Time = DateTime.UtcNow.AddDays(1),
            ImageUrl = $"https://test-image{suffix}.jpg",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Organizer = organizer,
            EventCategory = category,
            City = city
        };

        return (testEvent, category, city, organizer);
    }

    private async Task<EventSeatType> AddSeatType(ApplicationContext context, Event @event, decimal price)
    {
        var seatType = new EventSeatType
        {
            Id = Guid.NewGuid(),
            Name = $"Seat Type {price}",
            Price = (double)price,
            EventId = @event.Id,
            Event = @event
        };

        await context.EventSeatTypes.AddAsync(seatType);
        await context.SaveChangesAsync();

        return seatType;
    }

    private async Task<Ticket> AddTicket(ApplicationContext context, Event @event, EventSeatType seatType)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Event = @event,
            SeatingCategoryId = seatType.Id,
            SeatingCategory = seatType,
        };

        await context.Tickets.AddAsync(ticket);
        await context.SaveChangesAsync();

        return ticket;
    }

    [Fact]
    public async Task GetEvents_ReturnsOkResult_WithListOfEvents()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent1, _, _, _) = await SetupTestData(context, "1");
        var (testEvent2, _, _, _) = await SetupTestData(context, "2");

        await context.Events.AddRangeAsync(new[] { testEvent1, testEvent2 });
        await context.SaveChangesAsync();
        
        var controller = new EventController(context, _mockEnvironment.Object);
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.GetEvents();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = okResult.Value.Should().BeAssignableTo<List<Event>>().Subject;
        events.Should().HaveCount(2);
        events.Should().Contain(e => e.Id == testEvent1.Id);
        events.Should().Contain(e => e.Id == testEvent2.Id);
        
        controller.Response.Headers.Should().ContainKey("X-Total-Count");
        controller.Response.Headers.Should().ContainKey("X-Total-Pages");
        controller.Response.Headers["X-Total-Count"].ToString().Should().Be("2");
        controller.Response.Headers["X-Total-Pages"].ToString().Should().Be("1");
    }

    [Fact]
    public async Task GetEvents_WithInvalidPagination_UsesDefaultValues()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context);
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();
        
        var controller = new EventController(context, _mockEnvironment.Object);
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.GetEvents(pageNumber: -1, pageSize: -1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = okResult.Value.Should().BeAssignableTo<List<Event>>().Subject;
        events.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEvents_WithValidPagination_ReturnsCorrectPage()
    {
        // Arrange
        var context = CreateTestDbContext();
        var events = new List<Event>();
        
        // Create 15 events
        for (int i = 0; i < 15; i++)
        {
            var (testEvent, _, _, _) = await SetupTestData(context, $"page{i}");
            events.Add(testEvent);
        }

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();
        
        var controller = new EventController(context, _mockEnvironment.Object);
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act - Get second page with 5 items per page
        var result = await controller.GetEvents(pageNumber: 2, pageSize: 5);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedEvents = okResult.Value.Should().BeAssignableTo<List<Event>>().Subject;
        pagedEvents.Should().HaveCount(5);
        
        controller.Response.Headers.Should().ContainKey("X-Total-Count");
        controller.Response.Headers.Should().ContainKey("X-Total-Pages");
        controller.Response.Headers["X-Total-Count"].ToString().Should().Be("15");
        controller.Response.Headers["X-Total-Pages"].ToString().Should().Be("3");
    }

    [Fact]
    public async Task GetEventById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, category, city, organizer) = await SetupTestData(context, "unique");
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();
        
        var seatType = await AddSeatType(context, testEvent, 100m);
        
        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetEventById(testEvent.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEvent = okResult.Value.Should().BeOfType<EventDTO>().Subject;
        returnedEvent.Id.Should().Be(testEvent.Id);
        returnedEvent.Title.Should().Be(testEvent.Title);
        returnedEvent.Description.Should().Be(testEvent.Description);
        returnedEvent.Organizer.Should().Be(organizer.Name);
        returnedEvent.Category.Should().Be(category.Name);
        returnedEvent.City.Should().Be(city.Name);
        returnedEvent.Location.Should().Be(testEvent.Location);
        returnedEvent.SeatTypes.Should().HaveCount(1);
        returnedEvent.SeatTypes.First().Name.Should().Be(seatType.Name);
    }

    [Fact]
    public async Task GetEventById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await controller.GetEventById(nonExistentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(null, null, null, null, null, null, 1)] // No filters
    [InlineData("Test Event search", null, null, null, null, null, 1)] // Title only
    [InlineData(null, "Test Category search", null, null, null, null, 1)] // Category only
    [InlineData(null, null, "Test City search", null, null, null, 1)] // City only
    [InlineData(null, null, null, 50.0, null, null, 1)] // Min price only
    [InlineData(null, null, null, null, 150.0, null, 1)] // Max price only
    [InlineData("Test Event search", "Test Category search", "Test City search", 50.0, 150.0, null, 1)] // All filters
    public async Task Search_WithDifferentFilters_ReturnsFilteredEvents(
        string title, string category, string city, 
        double? minPrice, double? maxPrice, DateTime? dateTo,
        int expectedCount)
    {
        // Arrange
        var context = CreateTestDbContext();

        // Create test data with known values
        var testCategory = new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test Category search"
        };

        var testCity = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City search"
        };

        var testOrganizer = new Organizer
        {
            Id = Guid.NewGuid(),
            Name = "Test Organizer search",
            Phone = "1234567890",
            Email = "test.search@organizer.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.EventCategories.AddAsync(testCategory);
        await context.Cities.AddAsync(testCity);
        await context.Organizers.AddAsync(testOrganizer);
        await context.SaveChangesAsync();

        var testEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Event search",
            Description = "Test Description search",
            OrganizerId = testOrganizer.Id,
            CategoryId = testCategory.Id,
            Location = "Test Location search",
            CityId = testCity.Id,
            Time = DateTime.UtcNow.AddDays(1),
            ImageUrl = "https://test-image-search.jpg",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Organizer = testOrganizer,
            EventCategory = testCategory,
            City = testCity
        };

        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        // Add seat type with price 100
        var seatType = new EventSeatType
        {
            Id = Guid.NewGuid(),
            Name = "Test Seat Type search",
            Price = 100.0,
            EventId = testEvent.Id,
            Event = testEvent
        };

        await context.EventSeatTypes.AddAsync(seatType);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        var searchRequest = new EventSearchRequest
        {
            Title = title,
            Category = category,
            City = city,
            MinPrice = minPrice != null ? (decimal)minPrice : 0,
            MaxPrice = maxPrice != null ? (decimal)maxPrice : 1000,
            DateFrom = DateTime.UtcNow,
            DateTo = dateTo ?? DateTime.UtcNow.AddDays(2),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await controller.Search(searchRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = okResult.Value.Should().BeAssignableTo<List<SearchedEventDTO>>().Subject;
        events.Should().HaveCount(expectedCount);

        if (expectedCount > 0)
        {
            var firstEvent = events.First();
            firstEvent.Title.Should().Be(testEvent.Title);
            firstEvent.Location.Should().Be(testEvent.Location);
            firstEvent.Time.Should().Be(testEvent.Time);
        }
    }

    [Theory]
    [InlineData(1, 5)]  // First page
    [InlineData(2, 5)]  // Middle page
    [InlineData(3, 5)]  // Last page
    public async Task Search_WithPagination_ReturnsCorrectPage(int pageNumber, int pageSize)
    {
        // Arrange
        var context = CreateTestDbContext();
        var events = new List<Event>();
        
        // Create 15 events with the same category and city
        var (_, category, city, organizer) = await SetupTestData(context, "search_page");
        
        for (int i = 0; i < 15; i++)
        {
            var testEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = $"Test Event {i}",
                Description = $"Test Description {i}",
                OrganizerId = organizer.Id,
                CategoryId = category.Id,
                Location = $"Test Location {i}",
                CityId = city.Id,
                Time = DateTime.UtcNow.AddDays(1),
                ImageUrl = $"https://test-image{i}.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Organizer = organizer,
                EventCategory = category,
                City = city
            };
            events.Add(testEvent);
        }

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        var searchRequest = new EventSearchRequest
        {
            Category = category.Name,
            PageNumber = pageNumber,
            PageSize = pageSize,
            DateFrom = DateTime.UtcNow,
            DateTo = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.Search(searchRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var searchResults = okResult.Value.Should().BeAssignableTo<List<SearchedEventDTO>>().Subject;
        
        if (pageNumber == 3)  // Last page should have fewer items
        {
            searchResults.Should().HaveCount(5);
        }
        else
        {
            searchResults.Should().HaveCount(pageSize);
        }
    }

    [Fact]
    public async Task Search_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.Search(new EventSearchRequest());

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateEvent_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (_, category, city, organizer) = await SetupTestData(context);
        
        var controller = new EventController(context, _mockEnvironment.Object);
        var request = new CreateEventRequest
        {
            Title = "New Test Event",
            Description = "New Test Description",
            OrganizerId = organizer.Id,
            CategoryId = category.Id,
            CityId = city.Id,
            Location = "New Test Location",
            Time = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await controller.CreateEvent(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var createdEvent = createdResult.Value.Should().BeOfType<Event>().Subject;
        createdEvent.Title.Should().Be(request.Title.Trim());
        createdEvent.Description.Should().Be(request.Description.Trim());
    }

    [Theory]
    [InlineData(true, false, false, "Organizer with this Id doesn't exist.")]
    [InlineData(false, true, false, "Event Category with this Id doesn't exist.")]
    [InlineData(false, false, true, "City with this Id doesn't exist.")]
    public async Task CreateEvent_WithInvalidData_ReturnsBadRequest(
        bool useInvalidOrganizer, bool useInvalidCategory, bool useInvalidCity, string expectedError)
    {
        // Arrange
        var context = CreateTestDbContext();
        var (_, category, city, organizer) = await SetupTestData(context, "invalid");
        
        var controller = new EventController(context, _mockEnvironment.Object);
        var request = new CreateEventRequest
        {
            Title = "New Test Event",
            Description = "New Test Description",
            OrganizerId = useInvalidOrganizer ? Guid.NewGuid() : organizer.Id,
            CategoryId = useInvalidCategory ? Guid.NewGuid() : category.Id,
            CityId = useInvalidCity ? Guid.NewGuid() : city.Id,
            Location = "New Test Location",
            Time = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await controller.CreateEvent(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be(expectedError);
    }

    [Fact]
    public async Task CreateEvent_WithDuplicateEvent_ReturnsConflict()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, category, city, organizer) = await SetupTestData(context, "duplicate");
        
        // First, create the event
        var controller = new EventController(context, _mockEnvironment.Object);
        var request = new CreateEventRequest
        {
            Title = testEvent.Title,
            Description = testEvent.Description,
            OrganizerId = organizer.Id,
            CategoryId = category.Id,
            CityId = city.Id,
            Location = testEvent.Location,
            Time = testEvent.Time
        };

        await controller.CreateEvent(request);

        // Try to create the same event again
        var result = await controller.CreateEvent(request);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task UpdateEvent_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, category, city, organizer) = await SetupTestData(context);
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        var updateRequest = new UpdateEventRequest
        {
            Id = testEvent.Id,
            Title = "Updated Test Event",
            Description = "Updated Test Description",
            OrganizerId = organizer.Id,
            CategoryId = category.Id,
            CityId = city.Id,
            Location = "Updated Test Location",
            Time = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.UpdateEvent(updateRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var updatedEvent = okResult.Value.Should().BeOfType<Event>().Subject;
        updatedEvent.Title.Should().Be(updateRequest.Title.Trim());
        updatedEvent.Description.Should().Be(updateRequest.Description.Trim());
    }

    [Fact]
    public async Task UpdateEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);
        var updateRequest = new UpdateEventRequest
        {
            Id = Guid.NewGuid(),
            Title = "Updated Test Event",
            Description = "Updated Test Description",
            OrganizerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Location = "Updated Test Location",
            Time = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.UpdateEvent(updateRequest);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteEvent_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context);
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.DeleteEvent(testEvent.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var eventExists = await context.Events.AnyAsync(e => e.Id == testEvent.Id);
        eventExists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.DeleteEvent(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetFeaturedEvents_ReturnsCorrectNumberOfEvents()
    {
        // Arrange
        var context = CreateTestDbContext();
        var events = new List<Event>();
        
        // Create 10 events with different categories
        for (int i = 0; i < 10; i++)
        {
            var (testEvent, _, _, _) = await SetupTestData(context, $"featured{i}");
            events.Add(testEvent);
        }

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        // Add seat types and tickets after events are saved
        foreach (var testEvent in events)
        {
            var seatType = await AddSeatType(context, testEvent, 100m);
            await AddTicket(context, testEvent, seatType);
        }
        
        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetFeaturedEvents(maxEvents: 6);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var featuredEvents = okResult.Value.Should().BeAssignableTo<List<FeaturedEventDTO>>().Subject;
        featuredEvents.Should().HaveCountLessOrEqualTo(6);
    }

    [Fact]
    public async Task GetSeatTypesForEvent_WithValidId_ReturnsSeatTypes()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context, "seattype");
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var seatType1 = await AddSeatType(context, testEvent, 100m);
        var seatType2 = await AddSeatType(context, testEvent, 200m);

        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetSeatTypesForEvent(testEvent.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var seatTypes = okResult.Value.Should().BeAssignableTo<List<ShortSeatTypeDTO>>().Subject;
        seatTypes.Should().HaveCount(2);
        seatTypes.Should().Contain(st => st.Name == seatType1.Name);
        seatTypes.Should().Contain(st => st.Name == seatType2.Name);
    }

    [Fact]
    public async Task GetSeatTypesForEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetSeatTypesForEvent(Guid.NewGuid());

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAvailableTickets_WithValidIds_ReturnsTickets()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context, "tickets");
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var seatType = await AddSeatType(context, testEvent, 100m);
        var ticket1 = await AddTicket(context, testEvent, seatType);
        var ticket2 = await AddTicket(context, testEvent, seatType);

        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetAvailableTickets(testEvent.Id, seatType.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tickets = okResult.Value.Should().BeAssignableTo<List<ShortTicketDTO>>().Subject;
        tickets.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAvailableTickets_WithInvalidEventId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetAvailableTickets(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    private class UploadPhotoResponse
    {
        public string ImageUrl { get; set; } = string.Empty;
    }

    [Fact]
    public async Task UploadEventPhoto_WithValidFile_ReturnsOkResult()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context, "photo");
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        
        // Create a temporary directory for the test
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var tempFilePath = Path.Combine(tempDir, "test.jpg");

        try
        {
            // Create a valid JPEG image
            using (var image = new Image<Rgba32>(100, 100))
            {
                await image.SaveAsJpegAsync(tempFilePath);
            }

            // Read the file into memory to avoid file locks
            byte[] fileBytes;
            using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileBytes);
            }

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(fileBytes.Length);
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(fileBytes));
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                   .Returns<Stream, CancellationToken>(async (stream, token) =>
                   {
                       using var memoryStream = new MemoryStream(fileBytes);
                       await memoryStream.CopyToAsync(stream, token);
                   });

            // Setup mock environment
            var imagesPath = Path.Combine(tempDir, "images", "events");
            Directory.CreateDirectory(imagesPath);
            _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);

            // Act
            var result = await controller.UploadEventPhoto(testEvent.Id, fileMock.Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().NotBeNull();

            // Get the image URL from the response
            var json = JsonConvert.SerializeObject(okResult.Value);
            var jObject = JObject.Parse(json);
            var imageUrl = jObject["imageUrl"]?.ToString();
            imageUrl.Should().NotBeNull();
            imageUrl.Should().StartWith("/images/events/");

            // Extract filename from the URL and verify the file exists
            var fileName = Path.GetFileName(imageUrl);
            var expectedFilePath = Path.Combine(imagesPath, fileName);
            File.Exists(expectedFilePath).Should().BeTrue();

            // Verify the image URL was updated in the database
            var updatedEvent = await context.Events.FindAsync(testEvent.Id);
            updatedEvent.Should().NotBeNull();
            updatedEvent!.ImageUrl.Should().Be(imageUrl);

            // Clean up the test file before deleting directory
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            // Clean up any files in the images directory
            if (Directory.Exists(imagesPath))
            {
                foreach (var file in Directory.GetFiles(imagesPath))
                {
                    File.Delete(file);
                }
            }
        }
        finally
        {
            try
            {
                // Cleanup - delete the temporary directory and all its contents
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch (IOException)
            {
                // If we can't delete immediately, schedule for deletion at process exit
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // Ignore any final cleanup errors
                }
            }
        }
    }

    [Theory]
    [InlineData(null, "No file uploaded.")]
    [InlineData(6 * 1024 * 1024, "File size exceeds 5MB limit.")]
    public async Task UploadEventPhoto_WithInvalidFile_ReturnsBadRequest(int? fileSize, string expectedError)
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context);
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        
        IFormFile? file = null;
        if (fileSize.HasValue)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(fileSize.Value);
            file = fileMock.Object;
        }

        // Act
        var result = await controller.UploadEventPhoto(testEvent.Id, file);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be(expectedError);
    }

    [Theory]
    [InlineData(true, false, false, "Organizer with this Id doesn't exist.")]
    [InlineData(false, true, false, "Event Category with this Id doesn't exist.")]
    [InlineData(false, false, true, "City with this Id doesn't exist.")]
    public async Task UpdateEvent_WithInvalidDependencies_ReturnsBadRequest(
        bool useInvalidOrganizer, bool useInvalidCategory, bool useInvalidCity, string expectedError)
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, category, city, organizer) = await SetupTestData(context);
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        var updateRequest = new UpdateEventRequest
        {
            Id = testEvent.Id,
            Title = "Updated Test Event",
            Description = "Updated Test Description",
            OrganizerId = useInvalidOrganizer ? Guid.NewGuid() : organizer.Id,
            CategoryId = useInvalidCategory ? Guid.NewGuid() : category.Id,
            CityId = useInvalidCity ? Guid.NewGuid() : city.Id,
            Location = "Updated Test Location",
            Time = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.UpdateEvent(updateRequest);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be(expectedError);
    }

    [Fact]
    public async Task UpdateEvent_WithDuplicateData_ReturnsConflict()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent1, category, city, organizer) = await SetupTestData(context, "1");
        var (testEvent2, _, _, _) = await SetupTestData(context, "2");
        await context.Events.AddRangeAsync(new[] { testEvent1, testEvent2 });
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        var updateRequest = new UpdateEventRequest
        {
            Id = testEvent2.Id,
            Title = testEvent1.Title,
            Description = testEvent1.Description,
            OrganizerId = organizer.Id,
            CategoryId = category.Id,
            CityId = city.Id,
            Location = testEvent1.Location,
            Time = testEvent1.Time
        };

        // Act
        var result = await controller.UpdateEvent(updateRequest);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task GetFeaturedEvents_WithNoEvents_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetFeaturedEvents();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var featuredEvents = okResult.Value.Should().BeAssignableTo<List<FeaturedEventDTO>>().Subject;
        featuredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeaturedEvents_WithEventsOutsideWindow_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context);
        testEvent.Time = DateTime.UtcNow.AddDays(31); // Outside the 30-day window
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetFeaturedEvents();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var featuredEvents = okResult.Value.Should().BeAssignableTo<List<FeaturedEventDTO>>().Subject;
        featuredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeaturedEvents_WithNoAvailableTickets_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context);
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        // Add seat type and sold ticket
        var seatType = await AddSeatType(context, testEvent, 100m);
        var ticket = await AddTicket(context, testEvent, seatType);

        // Mark ticket as purchased
        var purchase = new TicketPurchase
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            WasRefunded = false
        };
        await context.TicketPurchases.AddAsync(purchase);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);

        // Act
        var result = await controller.GetFeaturedEvents();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var featuredEvents = okResult.Value.Should().BeAssignableTo<List<FeaturedEventDTO>>().Subject;
        featuredEvents.Should().BeEmpty();
    }

    [Theory]
    [InlineData("test.txt", "text/plain")]
    [InlineData("test.pdf", "application/pdf")]
    [InlineData("test.doc", "application/msword")]
    public async Task UploadEventPhoto_WithUnsupportedFileType_ReturnsBadRequest(string fileName, string contentType)
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context, "invalid_type");
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1KB
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);

        // Act
        var result = await controller.UploadEventPhoto(testEvent.Id, fileMock.Object);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Unsupported file type. Please upload a .jpg, .jpeg, .png, .gif, or .webp file.");
    }

    [Fact]
    public async Task UploadEventPhoto_WithNonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventController(context, _mockEnvironment.Object);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        // Act
        var result = await controller.UploadEventPhoto(Guid.NewGuid(), fileMock.Object);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Event not found.");
    }

    [Fact]
    public async Task UploadEventPhoto_WithCorruptImageData_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateTestDbContext();
        var (testEvent, _, _, _) = await SetupTestData(context, "corrupt");
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mockEnvironment.Object);
        
        // Create corrupt image data
        var corruptImageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // Invalid JPEG header

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(corruptImageData.Length);
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(corruptImageData));
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
               .Returns<Stream, CancellationToken>(async (stream, token) =>
               {
                   using var memoryStream = new MemoryStream(corruptImageData);
                   await memoryStream.CopyToAsync(stream, token);
               });

        // Setup mock environment
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);

        try
        {
            // Act
            var result = await controller.UploadEventPhoto(testEvent.Id, fileMock.Object);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid image file.");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
} 
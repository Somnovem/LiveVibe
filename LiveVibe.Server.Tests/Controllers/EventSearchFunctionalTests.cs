using LiveVibe.Server.Controllers;
using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.Requests.Events;
using LiveVibe.Server.Models.DTOs.ModelDTOs;
using LiveVibe.Server.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using LiveVibe.Server.Models.DTOs.Shared;

namespace LiveVibe.Server.Tests.Controllers
{
    public class EventSearchFunctionalTests : TestBase
    {
        private async Task<(Event, EventCategory, City, Organizer, EventSeatType)> SetupTestEventWithPrice(
            ApplicationContext context,
            string eventName,
            string categoryName,
            string cityName,
            double price)
        {
            var category = new EventCategory
            {
                Id = Guid.NewGuid(),
                Name = categoryName
            };

            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = cityName
            };

            var organizer = new Organizer
            {
                Id = Guid.NewGuid(),
                Name = $"Test Organizer for {eventName}",
                Phone = "1234567890",
                Email = $"test_{eventName.ToLower()}@organizer.com",
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
                Title = eventName,
                Description = $"Description for {eventName}",
                OrganizerId = organizer.Id,
                CategoryId = category.Id,
                Location = $"Location for {eventName}",
                CityId = city.Id,
                Time = DateTime.UtcNow.AddDays(1),
                ImageUrl = $"https://test-image-{eventName.ToLower()}.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Organizer = organizer,
                EventCategory = category,
                City = city
            };

            await context.Events.AddAsync(testEvent);
            await context.SaveChangesAsync();

            var seatType = new EventSeatType
            {
                Id = Guid.NewGuid(),
                Name = $"Seat Type for {eventName}",
                Price = price,
                EventId = testEvent.Id,
                Event = testEvent
            };

            await context.EventSeatTypes.AddAsync(seatType);
            await context.SaveChangesAsync();

            return (testEvent, category, city, organizer, seatType);
        }

        [Fact]
        public async Task Search_WithMultipleFilters_ReturnsCorrectEvents()
        {
            // Arrange
            var context = CreateTestDbContext();
            
            // Setup test events with different prices and categories
            await SetupTestEventWithPrice(context, "Cheap Concert", "Концерт", "Київ", 50.0);
            await SetupTestEventWithPrice(context, "Expensive Concert", "Концерт", "Київ", 200.0);
            await SetupTestEventWithPrice(context, "Medium Festival", "Фестиваль", "Львів", 100.0);
            await SetupTestEventWithPrice(context, "Theater Show", "Вистава", "Одеса", 150.0);

            var controller = new EventController(context, null);

            // Act: Search for concerts in Kyiv with price between 0 and 100
            var searchRequest = new EventSearchRequest
            {
                Category = "Концерт",
                City = "Київ",
                MinPrice = 0,
                MaxPrice = 100,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(7),
                PageNumber = 1,
                PageSize = 10
            };

            var result = await controller.Search(searchRequest);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var events = okResult.Value.Should().BeAssignableTo<PagedListDTO<SearchedEventDTO>>().Subject;
            
            // Should only return the cheap concert
            events.Items.Should().HaveCount(1);
            events.Items[0].Title.Should().Be("Cheap Concert");
            events.Items[0].Category.Should().Be("Концерт");
            events.Items[0].City.Should().Be("Київ");
            events.Items[0].MatchedSeatCategoryPrice.Should().Be(50.0m);
        }

        [Fact]
        public async Task Search_WithDateRange_ReturnsEventsInRange()
        {
            // Arrange
            var context = CreateTestDbContext();
            var controller = new EventController(context, null);

            var now = DateTime.UtcNow;
            
            // Create events with different dates
            var (pastEvent, _, _, _, _) = await SetupTestEventWithPrice(context, "Past Event", "Концерт", "Київ", 100.0);
            pastEvent.Time = now.AddDays(-1);
            
            var (nearFutureEvent, _, _, _, _) = await SetupTestEventWithPrice(context, "Near Future Event", "Концерт", "Київ", 100.0);
            nearFutureEvent.Time = now.AddDays(2);
            
            var (farFutureEvent, _, _, _, _) = await SetupTestEventWithPrice(context, "Far Future Event", "Концерт", "Київ", 100.0);
            farFutureEvent.Time = now.AddDays(10);

            await context.SaveChangesAsync();

            // Act: Search for events in next 5 days
            var searchRequest = new EventSearchRequest
            {
                DateFrom = now,
                DateTo = now.AddDays(5),
                PageNumber = 1,
                PageSize = 10
            };

            var result = await controller.Search(searchRequest);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var events = okResult.Value.Should().BeAssignableTo<PagedListDTO<SearchedEventDTO>>().Subject;
            
            events.Items.Should().HaveCount(1);
            events.Items[0].Title.Should().Be("Near Future Event");
        }

        [Fact]
        public async Task Search_WithTitleFilter_ReturnsCaseInsensitiveMatches()
        {
            // Arrange
            var context = CreateTestDbContext();
            
            await SetupTestEventWithPrice(context, "Rock Concert 2024", "Концерт", "Київ", 100.0);
            await SetupTestEventWithPrice(context, "Pop Concert 2024", "Концерт", "Київ", 100.0);
            await SetupTestEventWithPrice(context, "Фестиваль джазу", "Фестиваль", "Львів", 100.0);

            var controller = new EventController(context, null);

            // Act: Search for events with "concert" in title
            var searchRequest = new EventSearchRequest
            {
                Title = "Concert",
                PageNumber = 1,
                PageSize = 10
            };

            var result = await controller.Search(searchRequest);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var events = okResult.Value.Should().BeAssignableTo<PagedListDTO<SearchedEventDTO>>().Subject;
            
            events.Items.Should().HaveCount(2);
            events.Items.Should().Contain(e => e.Title == "Rock Concert 2024");
            events.Items.Should().Contain(e => e.Title == "Pop Concert 2024");
        }
    }
} 
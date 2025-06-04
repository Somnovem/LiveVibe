using FluentAssertions;
using LiveVibe.Server.Controllers;
using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.Requests.EventCategories;
using LiveVibe.Server.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LiveVibe.Server.Tests.Controllers;

public class EventCategoryControllerTests : TestBase
{
    private async Task<EventCategory> SetupTestCategory(ApplicationContext context)
    {
        var category = new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test Category"
        };

        await context.EventCategories.AddAsync(category);
        await context.SaveChangesAsync();

        return category;
    }

    [Fact]
    public async Task GetEventCategories_ReturnsAllCategories()
    {
        // Arrange
        var context = CreateTestDbContext();
        var categories = new List<EventCategory>();
        
        // Create 15 categories
        for (int i = 0; i < 15; i++)
        {
            var category = await SetupTestCategory(context);
            categories.Add(category);
        }
        
        var controller = new EventCategoryController(context);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.GetEventCategories();

        // Assert
        var returnedCategories = result.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returnedCategories.Should().HaveCount(15);
        foreach (var category in categories)
        {
            returnedCategories.Should().Contain(category.Name);
        }
    }

    [Fact]
    public async Task GetEventCategoryById_WithValidId_ReturnsCategory()
    {
        // Arrange
        var context = CreateTestDbContext();
        var category = await SetupTestCategory(context);
        var controller = new EventCategoryController(context);

        // Act
        var result = await controller.GetEventCategoryById(category.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCategory = okResult.Value.Should().BeOfType<EventCategory>().Subject;
        returnedCategory.Id.Should().Be(category.Id);
        returnedCategory.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetEventCategoryById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventCategoryController(context);
        var invalidId = Guid.NewGuid();

        // Act
        var result = await controller.GetEventCategoryById(invalidId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateEventCategory_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventCategoryController(context);
        var request = new CreateEventCategoryRequest
        {
            Name = "New Test Category"
        };

        // Act
        var result = await controller.CreateEventCategory(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var category = createdResult.Value.Should().BeOfType<EventCategory>().Subject;
        category.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreateEventCategory_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var context = CreateTestDbContext();
        var existingCategory = await SetupTestCategory(context);
        var controller = new EventCategoryController(context);
        var request = new CreateEventCategoryRequest
        {
            Name = existingCategory.Name
        };

        // Act
        var result = await controller.CreateEventCategory(request);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task DeleteEventCategory_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var context = CreateTestDbContext();
        var category = await SetupTestCategory(context);
        var controller = new EventCategoryController(context);

        // Act
        var result = await controller.DeleteEventCategory(category.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var categoryExists = await context.EventCategories.AnyAsync(c => c.Id == category.Id);
        categoryExists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEventCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = new EventCategoryController(context);
        var invalidId = Guid.NewGuid();

        // Act
        var result = await controller.DeleteEventCategory(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
} 
using FluentAssertions;
using LiveVibe.Server.Controllers;
using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.Requests.Countries;
using LiveVibe.Server.Models.DTOs.Responses;
using LiveVibe.Server.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LiveVibe.Server.Tests.Controllers;

public class CityControllerTests : TestBase
{
    private CityController CreateController(ApplicationContext context)
    {
        return new CityController(context);
    }

    [Fact]
    public async Task GetCities_ReturnsAllCityNames()
    {
        // Arrange
        var context = CreateTestDbContext();
        context.Cities.AddRange(new List<City>
        {
            new City { Id = Guid.NewGuid(), Name = "Kyiv" },
            new City { Id = Guid.NewGuid(), Name = "Lviv" }
        });
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        // Act
        var result = await controller.GetCities();

        // Assert
        result.Should().BeEquivalentTo(new[] { "Kyiv", "Lviv" });
    }

    [Fact]
    public async Task GetCityById_ReturnsCity_WhenExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var city = new City { Id = Guid.NewGuid(), Name = "Odesa" };
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        // Act
        var result = await controller.GetCityById(city.Id);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedCity = okResult!.Value as City;
        returnedCity.Should().NotBeNull();
        returnedCity!.Id.Should().Be(city.Id);
        returnedCity.Name.Should().Be(city.Name);
    }

    [Fact]
    public async Task GetCityById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = CreateController(context);

        // Act
        var result = await controller.GetCityById(Guid.NewGuid());

        // Assert
        var notFound = result.Result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
        var error = notFound!.Value as ErrorDTO;
        error.Should().NotBeNull();
        error!.Message.Should().Be("City not found");
    }

    [Fact]
    public async Task CreateCity_CreatesCity_WhenValid()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = CreateController(context);
        var request = new CreateCityRequest { Name = "Dnipro" };

        // Act
        var result = await controller.CreateCity(request);

        // Assert
        var created = result.Result as CreatedAtActionResult;
        created.Should().NotBeNull();
        var city = created!.Value as City;
        city.Should().NotBeNull();
        city!.Name.Should().Be(request.Name);
        context.Cities.Any(c => c.Name == request.Name).Should().BeTrue();
    }

    [Fact]
    public async Task CreateCity_ReturnsConflict_WhenNameExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        context.Cities.Add(new City { Id = Guid.NewGuid(), Name = "Zaporizhzhia" });
        await context.SaveChangesAsync();
        var controller = CreateController(context);
        var request = new CreateCityRequest { Name = "Zaporizhzhia" };

        // Act
        var result = await controller.CreateCity(request);

        // Assert
        var conflict = result.Result as ConflictObjectResult;
        conflict.Should().NotBeNull();
        var error = conflict!.Value as ErrorDTO;
        error.Should().NotBeNull();
        error!.Message.Should().Be("A city with this name already exists.");
    }

    [Fact]
    public async Task UpdateCity_UpdatesCity_WhenValid()
    {
        // Arrange
        var context = CreateTestDbContext();
        var city = new City { Id = Guid.NewGuid(), Name = "OldName" };
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        var controller = CreateController(context);
        var request = new UpdateCityRequest { Id = city.Id, Name = "NewName" };

        // Act
        var result = await controller.UpdateCity(request);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var updatedCity = ok!.Value as City;
        updatedCity.Should().NotBeNull();
        updatedCity!.Id.Should().Be(city.Id);
        updatedCity.Name.Should().Be("NewName");
        context.Cities.Find(city.Id)?.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task UpdateCity_ReturnsNotFound_WhenCityDoesNotExist()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = CreateController(context);
        var request = new UpdateCityRequest { Id = Guid.NewGuid(), Name = "DoesNotExist" };

        // Act
        var result = await controller.UpdateCity(request);

        // Assert
        var notFound = result.Result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
        var error = notFound!.Value as ErrorDTO;
        error.Should().NotBeNull();
        error!.Message.Should().Contain("No city found with ID");
    }

    [Fact]
    public async Task DeleteCity_DeletesCity_WhenExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var city = new City { Id = Guid.NewGuid(), Name = "ToDelete" };
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        // Act
        var result = await controller.DeleteCity(city.Id);

        // Assert
        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        var success = ok!.Value as SuccessDTO;
        success.Should().NotBeNull();
        success!.Message.Should().Be("City deleted successfully.");
        context.Cities.Any(c => c.Id == city.Id).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCity_ReturnsNotFound_WhenCityDoesNotExist()
    {
        // Arrange
        var context = CreateTestDbContext();
        var controller = CreateController(context);

        // Act
        var result = await controller.DeleteCity(Guid.NewGuid());

        // Assert
        var notFound = result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
        var error = notFound!.Value as ErrorDTO;
        error.Should().NotBeNull();
        error!.Message.Should().Be("City not found.");
    }
} 
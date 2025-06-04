using LiveVibe.Server.Controllers;
using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Models.DTOs.Requests.Countries;
using LiveVibe.Server.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace LiveVibe.Server.Tests.Controllers
{
    public class CityControllerIntegrationTests : TestBase
    {
        [Fact]
        public async Task CreateCity_AddsCityToDatabase()
        {
            // Arrange
            var context = CreateTestDbContext();
            var controller = new CityController(context);
            var request = new CreateCityRequest { Name = "Інтеграційне місто" };

            // Act
            var createResult = await controller.CreateCity(request);

            // Assert
            var created = createResult.Result as CreatedAtActionResult;
            Assert.NotNull(created);

            var city = created.Value as City;
            Assert.NotNull(city);
            Assert.Equal("Інтеграційне місто", city.Name);

            Assert.True(context.Cities.Any(c => c.Name == "Інтеграційне місто"));
        }

        [Fact]
        public async Task GetCityById_ReturnsCity_WhenExists()
        {
            // Arrange
            var context = CreateTestDbContext();
            var city = new City { Id = System.Guid.NewGuid(), Name = "Місто для отримання" };
            context.Cities.Add(city);
            await context.SaveChangesAsync();
            var controller = new CityController(context);

            // Act
            var getResult = await controller.GetCityById(city.Id);

            // Assert
            var okResult = getResult.Result as OkObjectResult;
            Assert.NotNull(okResult);

            var fetchedCity = okResult.Value as City;
            Assert.NotNull(fetchedCity);
            Assert.Equal(city.Id, fetchedCity.Id);
            Assert.Equal(city.Name, fetchedCity.Name);
        }
    }
} 
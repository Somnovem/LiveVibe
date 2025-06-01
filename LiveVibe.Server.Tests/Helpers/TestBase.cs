using Microsoft.EntityFrameworkCore;
using LiveVibe.Server.Models.Tables;
using Moq;

namespace LiveVibe.Server.Tests.Helpers;

public abstract class TestBase
{
    protected ApplicationContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationContext(options);
    }

    protected Mock<ApplicationContext> CreateMockDbContext()
    {
        return new Mock<ApplicationContext>();
    }
} 
using Azure.Core;
using LiveVibe.Server.HelperClasses.SeederClasses;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LiveVibe.Server.HelperClasses
{
    public static class SeedData
    {

        private static ApplicationContext? _context = null;
        private static UserManager<User>? _userManager = null;
        private static RoleManager<IdentityRole<Guid>>? _roleManager = null;

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            _context = serviceProvider.GetRequiredService<ApplicationContext>();
            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedAdminsAsync();
            await SeedCountriesAsync();
            await SeedEventCategoriesAsync();
            await SeedOrganizersAsync();
            await SeedEventsAsync();
            await SeedEventSeatTypesAndTicketsAsync();
            await SeedTicketPurchasesAsync();
        }

        private static async Task SeedRolesAsync()
        {
            string[] roleNames = { "User", "Admin", "SuperAdmin" };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager!.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }
        }

        private static async Task SeedUsersAsync()
        {

            if (await _userManager!.FindByIdAsync(UserIds.User1.ToString()) != null) return;

            var user1 = new User
            {
                Id = UserIds.User1,
                UserName = "user1@example.com",
                Email = "user1@example.com",
                FirstName = "Firstname1",
                LastName = "Lastname1",
                Phone = "+380000000001",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
            };
            await _userManager.CreateAsync(user1, "User1@123");
            await _userManager.AddToRoleAsync(user1, "User");

            var user2 = new User
            {
                Id = UserIds.User2,
                UserName = "user2@example.com",
                Email = "user2@example.com",
                FirstName = "Firstname2",
                LastName = "Lastname2",
                Phone = "+380000000002",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
            };
            await _userManager.CreateAsync(user2, "User2@123");
            await _userManager.AddToRoleAsync(user2, "User");

            var user3 = new User
            {
                Id = UserIds.User3,
                UserName = "user3@example.com",
                Email = "user3@example.com",
                FirstName = "Firstname3",
                LastName = "Lastname3",
                Phone = "+380000000003",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
            };
            await _userManager.CreateAsync(user3, "User3@123");
            await _userManager.AddToRoleAsync(user3, "User");
        }

        private static async Task SeedAdminsAsync()
        {

            if (await _userManager!.FindByEmailAsync("admin@example.com") != null) return;

            var admin = new User
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
            };
            await _userManager.CreateAsync(admin, "QweAsdZxc_1");
            await _userManager.AddToRoleAsync(admin, "Admin");

            if (await _userManager!.FindByEmailAsync("superadmin@example.com") != null) return;

            var superAdminUser = new User
            {
                UserName = "superadmin",
                Email = "superadmin@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
            };
            await _userManager.CreateAsync(superAdminUser, "QweAsdZxc_!23");
            await _userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
        }

        private static async Task SeedCountriesAsync()
        {

            if (_context!.Countries.Any()) return;

            var countries = new List<Country>
                {
                    new() { Id = CountryIds.Ukraine, Name = "Ukraine" },
                    new() { Id = CountryIds.UnitedStates, Name = "United States" },
                    new() { Id = CountryIds.UnitedKingdom, Name = "United Kingdom" },
                    new() { Id = CountryIds.Germany, Name = "Germany" },
                    new() { Id = CountryIds.France, Name = "France" },
                    new() { Id = CountryIds.Australia, Name = "Australia" },
                    new() { Id = CountryIds.Japan, Name = "Japan" },
                    new() { Id = CountryIds.Brazil, Name = "Brazil" },
                    new() { Id = CountryIds.India, Name = "India" },
                    new() { Id = CountryIds.SouthAfrica, Name = "South Africa" },
                    new() { Id = CountryIds.Spain, Name = "Spain" },
                    new() { Id = CountryIds.Italy, Name = "Italy" },
                    new() { Id = CountryIds.Mexico, Name = "Mexico" },
                    new() { Id = CountryIds.SouthKorea, Name = "South Korea" },
                    new() { Id = CountryIds.China, Name = "China" },
                    new() { Id = CountryIds.Canada, Name = "Canada" },
                    new() { Id = CountryIds.Argentina, Name = "Argentina" },
                    new() { Id = CountryIds.Netherlands, Name = "Netherlands" },
                    new() { Id = CountryIds.Sweden, Name = "Sweden" },
                    new() { Id = CountryIds.Norway, Name = "Norway" },
                    new() { Id = CountryIds.Finland, Name = "Finland" },
                    new() { Id = CountryIds.Denmark, Name = "Denmark" },
                    new() { Id = CountryIds.Belgium, Name = "Belgium" },
                    new() { Id = CountryIds.Portugal, Name = "Portugal" },
                    new() { Id = CountryIds.NewZealand, Name = "New Zealand" },
                    new() { Id = CountryIds.Switzerland, Name = "Switzerland" },
                    new() { Id = CountryIds.Poland, Name = "Poland" },
                    new() { Id = CountryIds.Turkey, Name = "Turkey" },
                    new() { Id = CountryIds.Greece, Name = "Greece" },
                    new() { Id = CountryIds.Ireland, Name = "Ireland" },
                    new() { Id = CountryIds.CzechRepublic, Name = "Czech Republic" },
                    new() { Id = CountryIds.Austria, Name = "Austria" },
                    new() { Id = CountryIds.Israel, Name = "Israel" },
                    new() { Id = CountryIds.Singapore, Name = "Singapore" },
                    new() { Id = CountryIds.Malaysia, Name = "Malaysia" },
                    new() { Id = CountryIds.Egypt, Name = "Egypt" },
                    new() { Id = CountryIds.Nigeria, Name = "Nigeria" },
                    new() { Id = CountryIds.Kenya, Name = "Kenya" },
                    new() { Id = CountryIds.Thailand, Name = "Thailand" },
                    new() { Id = CountryIds.Philippines, Name = "Philippines" },
                };

            _context.Countries.AddRange(countries);
            await _context.SaveChangesAsync();
        }

        private static async Task SeedEventCategoriesAsync()
        {
            if (_context!.EventCategories.Any()) return;

            var categories = new List<EventCategory>
                {
                    new() { Id = EventCategoryIds.Concert, Name = "Concert" },
                    new() { Id = EventCategoryIds.Festival, Name = "Festival" },
                    new() { Id = EventCategoryIds.Conference, Name = "Conference" },
                    new() { Id = EventCategoryIds.Sports, Name = "Sports" },
                    new() { Id = EventCategoryIds.Theater, Name = "Theater" },
                    new() { Id = EventCategoryIds.Workshop, Name = "Workshop" },
                    new() { Id = EventCategoryIds.Exhibition, Name = "Exhibition" },
                    new() { Id = EventCategoryIds.Meetup, Name = "Meetup" },
                };

            _context.EventCategories.AddRange(categories);
            await _context.SaveChangesAsync();
        }

        private static async Task SeedOrganizersAsync()
        {

            if (_context!.Organizers.Any()) return;

            var now = DateTime.UtcNow;

            var organizers = new List<Organizer>
                {
                    new()
                    {
                        Id = OrganizerIds.LiveNation,
                        Name = "Live Nation",
                        Phone = "+1-800-123-4567",
                        Email = "info@livenation.com",
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new()
                    {
                        Id = OrganizerIds.EventCo,
                        Name = "EventCo",
                        Phone = "+1-800-987-6543",
                        Email = "contact@eventco.com",
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new()
                    {
                        Id = OrganizerIds.GlobalStage,
                        Name = "Global Stage",
                        Phone = "+1-555-765-4321",
                        Email = "support@globalstage.org",
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new()
                    {
                        Id = OrganizerIds.UrbanPulse,
                        Name = "Urban Pulse",
                        Phone = "+44-20-7946-1234",
                        Email = "events@urbanpulse.co.uk",
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new()
                    {
                        Id = OrganizerIds.ZenithEvents,
                        Name = "Zenith Events",
                        Phone = "+49-30-1234-5678",
                        Email = "info@zenithevents.de",
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                };

            _context.Organizers.AddRange(organizers);
            await _context.SaveChangesAsync();
        }

        private static async Task SeedEventsAsync()
        {

            if (_context!.Events.Any()) return;

            var now = DateTime.UtcNow;

            var events = new List<Event>
                {
                    new()
                    {
                        Id = EventIds.MusicFest,
                        Title = "Summer Music Fest 2025",
                        Description = "Join top artists in a weekend of music and fun.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Festival,
                        Location = "Central Park, New York",
                        CountryId = CountryIds.UnitedStates,
                        Time = now.AddMonths(1),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/a7c0b0ef-9e3f-4201-a8a0-31b19c98f6c4.jpg"
                    },
                    new()
                    {
                        Id = EventIds.FoodCarnival,
                        Title = "Global Food Carnival",
                        Description = "Taste dishes from around the world.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Festival,
                        Location = "Hyde Park, London",
                        CountryId = CountryIds.UnitedKingdom,
                        Time = now.AddMonths(2),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/d942be63-67a1-44d4-bbf9-63e4b8f3e601.jpg"
                    },
                    new()
                    {
                        Id = EventIds.TechExpo,
                        Title = "Tech Expo Europe",
                        Description = "Latest innovations in AI, robotics, and more.",
                        OrganizerId = OrganizerIds.GlobalStage,
                        CategoryId = EventCategoryIds.Exhibition,
                        Location = "Berlin Messe, Berlin",
                        CountryId = CountryIds.Germany,
                        Time = now.AddMonths(3),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/5bc7ae20-104d-4388-86cb-d9f451b1c5f3.jpg"
                    },
                    new()
                    {
                        Id = EventIds.ArtFair,
                        Title = "International Art Fair",
                        Description = "Exhibitions from global artists.",
                        OrganizerId = OrganizerIds.UrbanPulse,
                        CategoryId = EventCategoryIds.Exhibition,
                        Location = "Louvre Museum, Paris",
                        CountryId = CountryIds.France,
                        Time = now.AddMonths(2),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/2de574a8-7606-4cd3-b87d-ef6d0a1b4d86.jpg"
                    },
                    new()
                    {
                        Id = EventIds.JazzNight,
                        Title = "Jazz Night Live",
                        Description = "An evening of live jazz performances.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Opera House, Sydney",
                        CountryId = CountryIds.Australia,
                        Time = now.AddDays(20),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/3e1cb2e4-1889-41da-bb63-7dc1858b8790.jpg"
                    },
                    new()
                    {
                        Id = EventIds.BroadwayPlay,
                        Title = "Broadway's Phantom Revival",
                        Description = "An exclusive return of Phantom of the Opera on Broadway.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Majestic Theatre, New York",
                        CountryId = CountryIds.UnitedStates,
                        Time = now.AddMonths(1).AddDays(10),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/c8f39b1e-32db-4abf-a922-8aa979d6fc38.jpg"
                    },
                    new()
                    {
                        Id = EventIds.ShakespeareNight,
                        Title = "Shakespeare Night: Hamlet Live",
                        Description = "Experience Hamlet as it was meant to be—live on stage.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Taras Shevchenko National Opera, Kyiv",
                        CountryId = CountryIds.Ukraine,
                        Time = now.AddMonths(-2),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/f4117d94-f1f1-44a3-a50e-8b2be25ff437.jpg"
                    },

                };

            _context.Events.AddRange(events);
            await _context.SaveChangesAsync();
        }

        public static async Task SeedEventSeatTypesAndTicketsAsync()
        {
            if (_context!.EventSeatTypes.Any()) return;

            var seatTypes = new List<EventSeatType>
            {
                new()
                {
                    Id = EventSeatTypeIds.MusicFestStandard,
                    EventId = EventIds.MusicFest,
                    Name = "Standard",
                    Capacity = 800,
                    AvailableSeats = 800,
                    Price = 60.00
                },
                new()
                {
                    Id = EventSeatTypeIds.MusicFestTable,
                    EventId = EventIds.MusicFest,
                    Name = "Table",
                    Capacity = 100,
                    AvailableSeats = 100,
                    Price = 200.00
                },
                new()
                {
                    Id = EventSeatTypeIds.FoodCarnivalStandard,
                    EventId = EventIds.FoodCarnival,
                    Name = "Standard",
                    Capacity = 1000,
                    AvailableSeats = 1000,
                    Price = 75.00
                },
                new()
                {
                    Id = EventSeatTypeIds.FoodCarnivalTable,
                    EventId = EventIds.FoodCarnival,
                    Name = "Table",
                    Capacity = 150,
                    AvailableSeats = 150,
                    Price = 175.00
                },
                new()
                {
                    Id = EventSeatTypeIds.TechExpoStandard,
                    EventId = EventIds.TechExpo,
                    Name = "Standard",
                    Capacity = 450,
                    AvailableSeats = 450,
                    Price = 80.00
                },
                new()
                {
                    Id = EventSeatTypeIds.TechExpoVIP,
                    EventId = EventIds.TechExpo,
                    Name = "VIP",
                    Capacity = 100,
                    AvailableSeats = 100,
                    Price = 150.00
                },
                new()
                {
                    Id = EventSeatTypeIds.ArtFairStandard,
                    EventId = EventIds.ArtFair,
                    Name = "Standard",
                    Capacity = 400,
                    AvailableSeats = 400,
                    Price = 60.00
                },
                new()
                {
                    Id = EventSeatTypeIds.ArtFairVIP,
                    EventId = EventIds.ArtFair,
                    Name = "VIP",
                    Capacity = 50,
                    AvailableSeats = 50,
                    Price = 200.00
                },
                new()
                {
                    Id = EventSeatTypeIds.JazzNightStandard,
                    EventId = EventIds.JazzNight,
                    Name = "Standard",
                    Capacity = 350,
                    AvailableSeats = 350,
                    Price = 65.00
                },
                new()
                {
                    Id = EventSeatTypeIds.JazzNightBalcony,
                    EventId = EventIds.JazzNight,
                    Name = "Balcony",
                    Capacity = 150,
                    AvailableSeats = 150,
                    Price = 170.00
                },
                new()
                {
                    Id = EventSeatTypeIds.BroadwayStandard,
                    EventId = EventIds.BroadwayPlay,
                    Name = "Standard",
                    Capacity = 200,
                    AvailableSeats = 200,
                    Price = 75.00
                },
                new()
                {
                    Id = EventSeatTypeIds.BroadwayVIP,
                    EventId = EventIds.BroadwayPlay,
                    Name = "VIP",
                    Capacity = 50,
                    AvailableSeats = 50,
                    Price = 150.00
                },
                new()
                {
                    Id = EventSeatTypeIds.ShakespeareStandard,
                    EventId = EventIds.ShakespeareNight,
                    Name = "Standard",
                    Capacity = 180,
                    AvailableSeats = 180,
                    Price = 60.00
                },
                new()
                {
                    Id = EventSeatTypeIds.ShakespeareBalcony,
                    EventId = EventIds.ShakespeareNight,
                    Name = "Balcony",
                    Capacity = 70,
                    AvailableSeats = 70,
                    Price = 45.00
                }
            };

            foreach (var seatType in seatTypes)
            {
                _context.EventSeatTypes.Add(seatType);

                for (int i = 1; i <= seatType.Capacity; i++)
                {
                    var ticket = new Ticket
                    {
                        Id = Guid.NewGuid(),
                        EventId = seatType.EventId,
                        SeatingCategoryId = seatType.Id,
                        Seat = $"{seatType.Name}-{i:D3}"
                    };

                    _context.Tickets.Add(ticket);
                }
            }

            _context.EventSeatTypes.AddRange(seatTypes);
            await _context.SaveChangesAsync();
        }

        public static async Task SeedTicketPurchasesAsync()
        {
            if (_context!.TicketPurchases.Any()) return;

            var now = DateTime.UtcNow;

            var ticket1 = await _context.Tickets
                .Where(t => t.EventId == EventIds.MusicFest && t.SeatingCategoryId == EventSeatTypeIds.MusicFestStandard && t.Seat == "Standard-001")
                .FirstOrDefaultAsync();

            var ticket2 = await _context.Tickets
                .Where(t => t.EventId == EventIds.MusicFest && t.SeatingCategoryId == EventSeatTypeIds.MusicFestTable && t.Seat == "Table-001")
                .FirstOrDefaultAsync();

            var ticket3 = await _context.Tickets
                .Where(t => t.EventId == EventIds.FoodCarnival && t.SeatingCategoryId == EventSeatTypeIds.FoodCarnivalStandard && t.Seat == "Standard-001")
                .FirstOrDefaultAsync();

            var ticket4 = await _context.Tickets
                .Where(t => t.EventId == EventIds.FoodCarnival && t.SeatingCategoryId == EventSeatTypeIds.FoodCarnivalTable && t.Seat == "Table-001")
                .FirstOrDefaultAsync();

            if (ticket1 == null || ticket2 == null || ticket3 == null || ticket4 == null)
                throw new Exception("Seed tickets not found, check ticket seeding!");

            var ticketPurchases = new List<TicketPurchase>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User1,
                    TicketId = ticket1.Id,
                    CreatedAt = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User1,
                    TicketId = ticket2.Id,
                    CreatedAt = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User2,
                    TicketId = ticket3.Id,
                    CreatedAt = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User3,
                    TicketId = ticket4.Id,
                    CreatedAt = now
                }
            };

            _context.TicketPurchases.AddRange(ticketPurchases);
            await _context.SaveChangesAsync();
        }
    }
}

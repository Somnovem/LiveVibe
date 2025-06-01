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
            await SeedCitiesAsync();
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

        private static async Task SeedCitiesAsync()
        {
            if (_context!.Cities.Any()) return;

            var cities = new List<City>
                {
                    new() { Id = CityIds.Kyiv, Name = "Київ" },
                    new() { Id = CityIds.Kharkiv, Name = "Харків" },
                    new() { Id = CityIds.Odesa, Name = "Одеса" },
                    new() { Id = CityIds.Dnipro, Name = "Дніпро" },
                    new() { Id = CityIds.Donetsk, Name = "Донецьк" },
                    new() { Id = CityIds.Zaporizhzhia, Name = "Запоріжжя" },
                    new() { Id = CityIds.Lviv, Name = "Львів" },
                    new() { Id = CityIds.Kryvyi_Rih, Name = "Кривий Ріг" },
                    new() { Id = CityIds.Mykolaiv, Name = "Миколаїв" },
                    new() { Id = CityIds.Mariupol, Name = "Маріуполь" },
                    new() { Id = CityIds.Luhansk, Name = "Луганськ" },
                    new() { Id = CityIds.Vinnytsia, Name = "Вінниця" },
                    new() { Id = CityIds.Makiivka, Name = "Макіївка" },
                    new() { Id = CityIds.Sevastopol, Name = "Севастополь" },
                    new() { Id = CityIds.Simferopol, Name = "Сімферополь" },
                    new() { Id = CityIds.Chernivtsi, Name = "Чернівці" },
                    new() { Id = CityIds.Poltava, Name = "Полтава" },
                    new() { Id = CityIds.Kherson, Name = "Херсон" },
                    new() { Id = CityIds.Cherkasy, Name = "Черкаси" },
                    new() { Id = CityIds.Khmelnytskyi, Name = "Хмельницький" },
                    new() { Id = CityIds.Zhytomyr, Name = "Житомир" },
                    new() { Id = CityIds.Sumy, Name = "Суми" },
                    new() { Id = CityIds.Rivne, Name = "Рівне" },
                    new() { Id = CityIds.Ivano_Frankivsk, Name = "Івано-Франківськ" },
                    new() { Id = CityIds.Ternopil, Name = "Тернопіль" },
                    new() { Id = CityIds.Lutsk, Name = "Луцьк" },
                    new() { Id = CityIds.Uzhhorod, Name = "Ужгород" }
                };

            _context.Cities.AddRange(cities);
            await _context.SaveChangesAsync();
        }

        private static async Task SeedEventCategoriesAsync()
        {
            if (_context!.EventCategories.Any()) return;

            var categories = new List<EventCategory>
                {
                    new() { Id = EventCategoryIds.Concert, Name = "Концерт" },
                    new() { Id = EventCategoryIds.Festival, Name = "Фестиваль" },
                    new() { Id = EventCategoryIds.Conference, Name = "З'їзд" },
                    new() { Id = EventCategoryIds.Sports, Name = "Спортивний захід" },
                    new() { Id = EventCategoryIds.Theater, Name = "Вистава" },
                    new() { Id = EventCategoryIds.Workshop, Name = "Інтерактив" },
                    new() { Id = EventCategoryIds.Exhibition, Name = "Виставка" },
                    new() { Id = EventCategoryIds.Standup, Name = "Стендап" },
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
                        Title = "ОДИН В КАНОЕ",
                        Description = "Магія звучання, Щирість замість байдужості, Осінь, що відчувається влітку, Вулиця, яка лишається пустою опівночі, Тиша, що нагадує запитання без відповіді, Музика, де кожна пауза - це нота, Подих - кожен, немов останній. Проживемо все це разом із вами.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "МЦКМ (Жовтневий палац)",
                        CityId = CityIds.Kyiv,
                        Time = now.AddMonths(1),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/a7c0b0ef-9e3f-4201-a8a0-31b19c98f6c4.jpg"
                    },
                    new()
                    {
                        Id = EventIds.FoodCarnival,
                        Title = "Серцеїдки",
                        Description = "На вас чекає феєрверк яскравих акторських робіт у супроводі музики та танців. Вистава, що надихатиме вас на справжні почуття.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Центральний будинок збройних сил України",
                        CityId = CityIds.Kyiv,
                        Time = now.AddMonths(2),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/d942be63-67a1-44d4-bbf9-63e4b8f3e601.jpg"
                    },
                    new()
                    {
                        Id = EventIds.TechExpo,
                        Title = "Стендап на трьох",
                        Description = "Стендап На Трьох - це івент, який обіцяє вам незабутній досвід сміху і розваг! Це місце, де досвідчені коміки зберуться, щоб перевірити свій новий матеріал зал і зарядити вас позитивною енергією.",
                        OrganizerId = OrganizerIds.GlobalStage,
                        CategoryId = EventCategoryIds.Standup,
                        Location = "Бочка Pub",
                        CityId = CityIds.Kyiv,
                        Time = now.AddMonths(3),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/5bc7ae20-104d-4388-86cb-d9f451b1c5f3.jpg"
                    },
                    new()
                    {
                        Id = EventIds.ArtFair,
                        Title = "Леви на Джипі. Birthday Tour",
                        Description = "Шість років нашому проєкту і ми хочемо цю дату розділити у вашому місті разом з нашими підписниками. Коміки Микола Зирянов, Валік Міхієнко, Роман Щербан та Костя Трембовецький імпровізуватимуть з залом про загальні речі та конкретні теми, які будуть обговорюватись в окремому Телеграм-каналі кожного концерту. Частина відповідей та голосувань буде народжуватись під час розмови з вами. До зустрічі у вашому місті, святкуємо день народження разом!\r\nТакож шукайте в своїх електронних квиточках посилання на окремий Телеграм-чат концерту, воно буде біля штрих-коду.",
                        OrganizerId = OrganizerIds.UrbanPulse,
                        CategoryId = EventCategoryIds.Standup,
                        Location = "Івано-Франківський театр ім. І. Франка",
                        CityId = CityIds.Ivano_Frankivsk,
                        Time = now.AddMonths(2),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/2de574a8-7606-4cd3-b87d-ef6d0a1b4d86.jpg"
                    },
                    new()
                    {
                        Id = EventIds.JazzNight,
                        Title = "СУСІДИ СТЕРПЛЯТЬ",
                        Description = "Довгоочікуваний концерт вашого улюбленого гурту.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Італійський Двір Одеської Філармонії",
                        CityId = CityIds.Odesa,
                        Time = now.AddDays(20),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/3e1cb2e4-1889-41da-bb63-7dc1858b8790.jpg"
                    },
                    new()
                    {
                        Id = EventIds.BroadwayPlay,
                        Title = "Конотопська відьма",
                        Description = "Вистава отримала неоднозначні відгуки та реакцію як від критиків, так і від аудиторії. Деякі вважають, що вона сміливо порушує важливі теми, тоді як інші критикують її за надмірну експериментальність. Однак усі погоджуються, що «Конотопська відьма» є важливим та неоціненим внеском у розвиток та розуміння сучасного українського театру, який намагається переосмислити традиційні культурні наративи та розкрити нові соціальні значення.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Чернівецький музично-драматичний театр ім. О. Кобилянської",
                        CityId = CityIds.Chernivtsi,
                        Time = now.AddMonths(1).AddDays(10),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/c8f39b1e-32db-4abf-a922-8aa979d6fc38.jpg"
                    },
                    new()
                    {
                        Id = EventIds.ShakespeareNight,
                        Title = "Віктор Павлік & Великий Симфонічний Оркестр",
                        Description = "Віктор Павлік & Великий Симфонічний Оркестр\r\n\r\nЮвілейний концерт в Тернополі!\r\n\r\nЦього року народному артисту України Віктору Павліку виповнюється 60 років. І він відзначає свій ювілей так, як личить справжньому маестро – великим концертом у супроводі симфонічного оркестру на сцені  ПК Березіль\r\n\r\nЦе буде не просто концерт – це музична подорож крізь роки, спогади та почуття. Віктор Павлік – легенда української естради, артист, який подарував кільком поколінням слухачів щирість і справжню музику. Його голос – це емоція, це історія, це душа.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "ПК Березіль",
                        CityId = CityIds.Ternopil,
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

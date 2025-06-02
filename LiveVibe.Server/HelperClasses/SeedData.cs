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
                    new() { Id = EventCategoryIds.Theater, Name = "Вистава" },
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
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444401"),
                        Title = "ОДИН В КАНОЕ",
                        Description = "Магія звучання, Щирість замість байдужості, Осінь, що відчувається влітку, Вулиця, яка лишається пустою опівночі, Тиша, що нагадує запитання без відповіді, Музика, де кожна пауза - це нота, Подих - кожен, немов останній. Проживемо все це разом із вами.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "МЦКМ (Жовтневий палац)",
                        CityId = CityIds.Kyiv,
                        Time = now.AddMonths(1),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444402"),
                        Title = "FANCON 2025",
                        Description = "Найбільший фестиваль попкультури України повертається! Очікуйте фантастичний мікс світів і культур, жанрів і форматів, кольорів та образів.",
                        OrganizerId = OrganizerIds.UrbanPulse,
                        CategoryId = EventCategoryIds.Festival,
                        Location = "Міжнародний Виставковий Центр",
                        CityId = CityIds.Kyiv,
                        Time = new DateTime(2025, 6, 7, 10, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444403"),
                        Title = "Океан Ельзи. Той день у Львові",
                        Description = "Легендарний український гурт Океан Ельзи з великим концертом у Львові.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Територія !FESTrepublic",
                        CityId = CityIds.Lviv,
                        Time = new DateTime(2025, 6, 27, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444404"),
                        Title = "MONATIK. Так вмієш лиш ти",
                        Description = "Неймовірне шоу від MONATIK у Палаці спорту.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Палац спорту",
                        CityId = CityIds.Kyiv,
                        Time = new DateTime(2025, 9, 20, 20, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444405"),
                        Title = "СУСІДИ СТЕРПЛЯТЬ",
                        Description = "Довгоочікуваний концерт вашого улюбленого гурту.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Італійський Двір Одеської Філармонії",
                        CityId = CityIds.Odesa,
                        Time = now.AddDays(20),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444406"),
                        Title = "Конотопська відьма",
                        Description = "Вистава отримала неоднозначні відгуки та реакцію як від критиків, так і від аудиторії. Деякі вважають, що вона сміливо порушує важливі теми, тоді як інші критикують її за надмірну експериментальність. Однак усі погоджуються, що «Конотопська відьма» є важливим та неоціненим внеском у розвиток та розуміння сучасного українського театру, який намагається переосмислити традиційні культурні наративи та розкрити нові соціальні значення.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Чернівецький музично-драматичний театр ім. О. Кобилянської",
                        CityId = CityIds.Chernivtsi,
                        Time = now.AddMonths(1).AddDays(10),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444407"),
                        Title = "Віктор Павлік & Великий Симфонічний Оркестр",
                        Description = "Віктор Павлік & Великий Симфонічний Оркестр. Ювілейний концерт в Тернополі! Цього року народному артисту України Віктору Павліку виповнюється 60 років. І він відзначає свій ювілей так, як личить справжньому маестро – великим концертом у супроводі симфонічного оркестру на сцені ПК Березіль. Це буде не просто концерт – це музична подорож крізь роки, спогади та почуття. Віктор Павлік – легенда української естради, артист, який подарував кільком поколінням слухачів щирість і справжню музику. Його голос – це емоція, це історія, це душа.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "ПК Березіль",
                        CityId = CityIds.Ternopil,
                        Time = now.AddMonths(-2),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444408"),
                        Title = "KAZKA",
                        Description = "Популярний український гурт KAZKA з концертом у Дніпрі.",
                        OrganizerId = OrganizerIds.GlobalStage,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Філармонія імені Л. Когана",
                        CityId = CityIds.Dnipro,
                        Time = new DateTime(2025, 7, 15, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa8.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444409"),
                        Title = "Dakh Daughters",
                        Description = "Унікальне театрально-музичне шоу від Dakh Daughters у Києві.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "МЦКМ (Жовтневий палац)",
                        CityId = CityIds.Kyiv,
                        Time = new DateTime(2025, 9, 19, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa9.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444410"),
                        Title = "МУР. Мюзикл Ти [РОМАНТИКА]",
                        Description = "Мюзикл 'Ти [РОМАНТИКА]' у Києві.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Центральний будинок ЗСУ",
                        CityId = CityIds.Kyiv,
                        Time = new DateTime(2025, 6, 6, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444411"),
                        Title = "Анна Трінчер",
                        Description = "Сольний концерт Анни Трінчер у Львові.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Львівська обласна філармонія",
                        CityId = CityIds.Lviv,
                        Time = new DateTime(2025, 7, 10, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa11.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444412"),
                        Title = "SHUMEI",
                        Description = "Концерт SHUMEI у Львові.",
                        OrganizerId = OrganizerIds.GlobalStage,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Malevich Concert Arena",
                        CityId = CityIds.Lviv,
                        Time = new DateTime(2025, 8, 5, 20, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa12.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444413"),
                        Title = "LATEXFAUNA у Львові",
                        Description = "Концерт українського інді-гурту LATEXFAUNA на терасі !FESTrepublic.",
                        OrganizerId = OrganizerIds.UrbanPulse,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Тераса !FESTrepublic",
                        CityId = CityIds.Lviv,
                        Time = new DateTime(2025, 6, 21, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa13.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444414"),
                        Title = "SYMPHONY OF ANIME в Одесі",
                        Description = "Найвідоміші мелодії з аніме у симфонічному виконанні в Одесі.",
                        OrganizerId = OrganizerIds.GlobalStage,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Театр музкомедії ім. Водяного",
                        CityId = CityIds.Odesa,
                        Time = new DateTime(2025, 7, 20, 18, 30, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa14.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444415"),
                        Title = "The Game of Thrones Concert у Харкові",
                        Description = "Музичне шоу за мотивами серіалу 'Гра престолів' у Golden Hall.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Golden Hall",
                        CityId = CityIds.Kharkiv,
                        Time = new DateTime(2025, 6, 15, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa15.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444416"),
                        Title = "FAINE MISTO: На зламі епох",
                        Description = "Фестиваль альтернативної музики у Львові — FAINE MISTO.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Festival,
                        Location = "!FESTrepublic",
                        CityId = CityIds.Lviv,
                        Time = new DateTime(2025, 8, 1, 11, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa16.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444417"),
                        Title = "Candlelight Concert у Харкові",
                        Description = "Концерт класичної музики при свічках у Golden Hall.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Golden Hall",
                        CityId = CityIds.Kharkiv,
                        Time = new DateTime(2025, 6, 21, 18, 30, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa17.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444418"),
                        Title = "Леви на Джипі. Birthday Tour",
                        Description = "Легендарний український гурт у святковому турі до Одеси.",
                        OrganizerId = OrganizerIds.UrbanPulse,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Будинок Клоунів",
                        CityId = CityIds.Odesa,
                        Time = new DateTime(2025, 6, 18, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa18.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444419"),
                        Title = "Святковий концерт у Дніпрі",
                        Description = "Традиційний український концерт у театрі ім. Т. Г. Шевченка.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Театр ім. Т. Г. Шевченка",
                        CityId = CityIds.Dnipro,
                        Time = new DateTime(2025, 6, 12, 18, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa19.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444420"),
                        Title = "DOMIY на біс! Криком з душі",
                        Description = "Повторна зустріч з улюбленим гуртом DOMIY у Львові.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "!FESTrepublic",
                        CityId = CityIds.Lviv,
                        Time = new DateTime(2025, 6, 7, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444421"),
                        Title = "Олександр Пономарьов та Михайло Хома у Харкові",
                        Description = "Ювілейний концерт дуету в Театрально-концертному центрі.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Театрально-концертний центр",
                        CityId = CityIds.Kharkiv,
                        Time = new DateTime(2025, 6, 9, 18, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa21.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444422"),
                        Title = "Курган & Agregat. Презентація альбому",
                        Description = "Експресивне реп-шоу в Харкові від культового тріо.",
                        OrganizerId = OrganizerIds.GlobalStage,
                        CategoryId = EventCategoryIds.Concert,
                        Location = "Loft Stage",
                        CityId = CityIds.Kharkiv,
                        Time = new DateTime(2025, 6, 11, 18, 30, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa22.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444423"),
                        Title = "Ревізор (М. Гоголь)",
                        Description = "Класична сатирична комедія за п'єсою Миколи Гоголя у виконанні акторів Львівського драмтеатру.",
                        OrganizerId = OrganizerIds.EventCo,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Львівський академічний драматичний театр ім. Лесі Українки",
                        CityId = CityIds.Lviv,
                        Time = new DateTime(2025, 6, 10, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa23.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444424"),
                        Title = "Украдене щастя (І. Франко)",
                        Description = "Трагедія пристрасті, кохання й боротьби за справедливість у постановці Одеського театру драми.",
                        OrganizerId = OrganizerIds.ZenithEvents,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Одеський національний академічний театр драми",
                        CityId = CityIds.Odesa,
                        Time = new DateTime(2025, 6, 14, 18, 30, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa24.jpg"
                    },
                    new()
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444425"),
                        Title = "Вій (за повістю Гоголя)",
                        Description = "Містичний спектакль за українською класикою з елементами хореографії та 3D-графіки.",
                        OrganizerId = OrganizerIds.LiveNation,
                        CategoryId = EventCategoryIds.Theater,
                        Location = "Харківський театр ім. Пушкіна",
                        CityId = CityIds.Kharkiv,
                        Time = new DateTime(2025, 6, 22, 19, 0, 0),
                        CreatedAt = now,
                        UpdatedAt = now,
                        ImageUrl = "/images/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa25.jpg"
                    }
                };

            _context.Events.AddRange(events);
            await _context.SaveChangesAsync();
        }

        public static async Task SeedEventSeatTypesAndTicketsAsync()
        {
            if (_context!.EventSeatTypes.Any()) return;

            var seatTypes = new List<EventSeatType>
            {
                // Event 1
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444401"), Name = "VIP", Capacity = 70, AvailableSeats = 70, Price = 1400.40 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444401"), Name = "Regular", Capacity = 194, AvailableSeats = 194, Price = 765.02 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444401"), Name = "Economy", Capacity = 257, AvailableSeats = 257, Price = 255.82 },
                
                // Event 2
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444402"), Name = "VIP", Capacity = 50, AvailableSeats = 50, Price = 1200.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444402"), Name = "Regular", Capacity = 150, AvailableSeats = 150, Price = 800.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444402"), Name = "Economy", Capacity = 200, AvailableSeats = 200, Price = 400.00 },

                // Event 3
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444403"), Name = "VIP", Capacity = 60, AvailableSeats = 60, Price = 1500.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444403"), Name = "Regular", Capacity = 180, AvailableSeats = 180, Price = 900.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444403"), Name = "Economy", Capacity = 240, AvailableSeats = 240, Price = 450.00 },

                // Event 4
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444404"), Name = "VIP", Capacity = 55, AvailableSeats = 55, Price = 1350.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444404"), Name = "Regular", Capacity = 165, AvailableSeats = 165, Price = 850.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444404"), Name = "Economy", Capacity = 220, AvailableSeats = 220, Price = 425.00 },

                // Event 5
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444405"), Name = "VIP", Capacity = 45, AvailableSeats = 45, Price = 1250.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444405"), Name = "Regular", Capacity = 155, AvailableSeats = 155, Price = 750.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444405"), Name = "Economy", Capacity = 210, AvailableSeats = 210, Price = 375.00 },

                // Event 6
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444406"), Name = "VIP", Capacity = 50, AvailableSeats = 50, Price = 1300.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444406"), Name = "Regular", Capacity = 160, AvailableSeats = 160, Price = 800.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444406"), Name = "Economy", Capacity = 215, AvailableSeats = 215, Price = 400.00 },

                // Event 7
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444407"), Name = "VIP", Capacity = 48, AvailableSeats = 48, Price = 1275.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444407"), Name = "Regular", Capacity = 158, AvailableSeats = 158, Price = 775.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444407"), Name = "Economy", Capacity = 213, AvailableSeats = 213, Price = 385.00 },

                // Event 8
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444408"), Name = "VIP", Capacity = 52, AvailableSeats = 52, Price = 1325.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444408"), Name = "Regular", Capacity = 162, AvailableSeats = 162, Price = 825.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444408"), Name = "Economy", Capacity = 217, AvailableSeats = 217, Price = 415.00 },

                // Event 9
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444409"), Name = "VIP", Capacity = 53, AvailableSeats = 53, Price = 1335.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444409"), Name = "Regular", Capacity = 163, AvailableSeats = 163, Price = 835.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444409"), Name = "Economy", Capacity = 218, AvailableSeats = 218, Price = 420.00 },

                // Event 10
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444410"), Name = "VIP", Capacity = 54, AvailableSeats = 54, Price = 1340.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444410"), Name = "Regular", Capacity = 164, AvailableSeats = 164, Price = 840.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444410"), Name = "Economy", Capacity = 219, AvailableSeats = 219, Price = 425.00 },

                // Event 11
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444411"), Name = "VIP", Capacity = 55, AvailableSeats = 55, Price = 1345.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444411"), Name = "Regular", Capacity = 165, AvailableSeats = 165, Price = 845.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444411"), Name = "Economy", Capacity = 220, AvailableSeats = 220, Price = 430.00 },

                // Event 12
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444412"), Name = "VIP", Capacity = 56, AvailableSeats = 56, Price = 1350.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444412"), Name = "Regular", Capacity = 166, AvailableSeats = 166, Price = 850.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444412"), Name = "Economy", Capacity = 221, AvailableSeats = 221, Price = 435.00 },

                // Event 13
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444413"), Name = "VIP", Capacity = 57, AvailableSeats = 57, Price = 1355.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444413"), Name = "Regular", Capacity = 167, AvailableSeats = 167, Price = 855.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444413"), Name = "Economy", Capacity = 222, AvailableSeats = 222, Price = 440.00 },

                // Event 14
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444414"), Name = "VIP", Capacity = 58, AvailableSeats = 58, Price = 1360.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444414"), Name = "Regular", Capacity = 168, AvailableSeats = 168, Price = 860.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444414"), Name = "Economy", Capacity = 223, AvailableSeats = 223, Price = 445.00 },

                // Event 15
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444415"), Name = "VIP", Capacity = 59, AvailableSeats = 59, Price = 1365.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444415"), Name = "Regular", Capacity = 169, AvailableSeats = 169, Price = 865.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444415"), Name = "Economy", Capacity = 224, AvailableSeats = 224, Price = 450.00 },

                // Event 16
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444416"), Name = "VIP", Capacity = 60, AvailableSeats = 60, Price = 1370.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444416"), Name = "Regular", Capacity = 170, AvailableSeats = 170, Price = 870.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444416"), Name = "Economy", Capacity = 225, AvailableSeats = 225, Price = 455.00 },

                // Event 17
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444417"), Name = "VIP", Capacity = 61, AvailableSeats = 61, Price = 1375.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444417"), Name = "Regular", Capacity = 171, AvailableSeats = 171, Price = 875.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444417"), Name = "Economy", Capacity = 226, AvailableSeats = 226, Price = 460.00 },

                // Event 18
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444418"), Name = "VIP", Capacity = 62, AvailableSeats = 62, Price = 1380.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444418"), Name = "Regular", Capacity = 172, AvailableSeats = 172, Price = 880.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444418"), Name = "Economy", Capacity = 227, AvailableSeats = 227, Price = 465.00 },

                // Event 19
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444419"), Name = "VIP", Capacity = 63, AvailableSeats = 63, Price = 1385.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444419"), Name = "Regular", Capacity = 173, AvailableSeats = 173, Price = 885.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444419"), Name = "Economy", Capacity = 228, AvailableSeats = 228, Price = 470.00 },

                // Event 20
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444420"), Name = "VIP", Capacity = 64, AvailableSeats = 64, Price = 1390.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444420"), Name = "Regular", Capacity = 174, AvailableSeats = 174, Price = 890.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444420"), Name = "Economy", Capacity = 229, AvailableSeats = 229, Price = 475.00 },

                // Event 21
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444421"), Name = "VIP", Capacity = 65, AvailableSeats = 65, Price = 1395.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444421"), Name = "Regular", Capacity = 175, AvailableSeats = 175, Price = 895.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444421"), Name = "Economy", Capacity = 230, AvailableSeats = 230, Price = 480.00 },

                // Event 22
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444422"), Name = "VIP", Capacity = 66, AvailableSeats = 66, Price = 1400.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444422"), Name = "Regular", Capacity = 176, AvailableSeats = 176, Price = 900.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444422"), Name = "Economy", Capacity = 231, AvailableSeats = 231, Price = 485.00 },

                // Event 23
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444423"), Name = "VIP", Capacity = 67, AvailableSeats = 67, Price = 1405.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444423"), Name = "Regular", Capacity = 177, AvailableSeats = 177, Price = 905.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444423"), Name = "Economy", Capacity = 232, AvailableSeats = 232, Price = 490.00 },

                // Event 24
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444424"), Name = "VIP", Capacity = 68, AvailableSeats = 68, Price = 1410.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444424"), Name = "Regular", Capacity = 178, AvailableSeats = 178, Price = 910.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444424"), Name = "Economy", Capacity = 233, AvailableSeats = 233, Price = 495.00 },

                // Event 25
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444425"), Name = "VIP", Capacity = 40, AvailableSeats = 40, Price = 1300.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444425"), Name = "Regular", Capacity = 160, AvailableSeats = 160, Price = 850.00 },
                new() { Id = Guid.NewGuid(), EventId = Guid.Parse("44444444-4444-4444-4444-444444444425"), Name = "Economy", Capacity = 220, AvailableSeats = 220, Price = 350.00 }
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

            var tickets = await _context.Tickets
                .AsNoTracking()
                .Include(t => t.SeatingCategory)
                .OrderBy(r => Guid.NewGuid())
                .Take(4)
                .ToListAsync();

            if (tickets.Count < 4)
                throw new Exception("Not enough tickets found in database!");

            var ticketPurchases = new List<TicketPurchase>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User1,
                    TicketId = tickets[0].Id,
                    CreatedAt = now,
                    PurchasePrice = Convert.ToDecimal(tickets[0].SeatingCategory.Price)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User1,
                    TicketId = tickets[1].Id,
                    CreatedAt = now,
                    PurchasePrice = Convert.ToDecimal(tickets[1].SeatingCategory.Price)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User2,
                    TicketId = tickets[2].Id,
                    CreatedAt = now,
                    PurchasePrice = Convert.ToDecimal(tickets[2].SeatingCategory.Price)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = UserIds.User3,
                    TicketId = tickets[3].Id,
                    CreatedAt = now,
                    PurchasePrice = Convert.ToDecimal(tickets[3].SeatingCategory.Price)
                }
            };

            _context.TicketPurchases.AddRange(ticketPurchases);
            await _context.SaveChangesAsync();
        }
    }
}

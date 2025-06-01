using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LiveVibe.Server.Models.Tables
{
    public class ApplicationContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Organizer> Organizers { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketPurchase> TicketPurchases { get; set; } = null!;
        public DbSet<EventSeatType> EventSeatTypes { get; set; } = null!;
        public DbSet<EventCategory> EventCategories { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TicketPurchase>()
                .ToTable("Ticket_Purchases", table => table.HasTrigger("trg_DecreaseSeatsOnPurchase"));
        }

    }
}
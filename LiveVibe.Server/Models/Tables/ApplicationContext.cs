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
        public DbSet<EventSeatType> EventSeatTypes { get; set; } = null!;
        public DbSet<EventCategory> EventCategories { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Order)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Disable OUTPUT clause for Orders table due to triggers
            modelBuilder.Entity<Order>()
                .ToTable("Orders", t => t.ExcludeFromMigrations())
                .HasAnnotation("SqlServer:UseSqlOutputClause", false);
        }
    }
}
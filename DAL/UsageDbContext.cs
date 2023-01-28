using Entities;
using Entities.Hotels;
using Entities.Transports;
using Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DAL
{
    public class UsageDbContext : IdentityDbContext<Customer, Role, int>
    {
        public UsageDbContext()
               : base()
        {
            Database.EnsureCreated();
        }
        public UsageDbContext(DbContextOptions<UsageDbContext> options)
               : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<HotelRoomReservation> HotelRoomReservations { get; set; }
        public DbSet<TransportTicket> TransportTickets { get; set; }
        public DbSet<Tour> TourTemplates { get; set; }
        public DbSet<Bill> Bills { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            var builder = new ConfigurationBuilder();

            builder.SetBasePath(Directory.GetCurrentDirectory());

            builder.AddJsonFile("appsettings.json");

            var config = builder.Build();

            string connectionString = config.GetConnectionString("UsageConnection");

            optionsBuilder
            .UseSqlServer(connectionString)
            .UseLazyLoadingProxies();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hotel>()
                .HasMany(r => r.Rooms)
                .WithOne(h => h.Hotel)
                .HasForeignKey(h => h.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transport>()
                .HasMany(t => t.TransportPlaces)
                .WithOne(t => t.Transport)
                .HasForeignKey(t => t.TransportId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transport>()
                .HasOne(t => t.Tour)
                .WithOne(t => t.TransportIn)
                .HasForeignKey<Tour>(t => t.TransportInId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Tour>()
                .HasOne(t => t.TransportIn)
                .WithOne(t => t.Tour)
                .HasForeignKey<Tour>(t => t.TransportInId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.PassportData)
                .WithOne(p => p.Customer)
                .HasForeignKey<PassportData>(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bill>()
                .HasOne(c => c.CustomerWhoBook)
                .WithMany(p => p.Bills)
                .HasForeignKey(p => p.CustomerWhoBookId)
                .OnDelete(DeleteBehavior.Cascade);


            base.OnModelCreating(modelBuilder);
        }
    }
}


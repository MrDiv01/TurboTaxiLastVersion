using Microsoft.EntityFrameworkCore;
using TurboTaxi.Domain.Entities;

namespace TurboTaxi.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<DriverStatusHistory> DriverStatusHistories => Set<DriverStatusHistory>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Ride> Rides => Set<Ride>();
        public DbSet<RideStatusHistory> RideStatusHistories => Set<RideStatusHistory>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
        public DbSet<UserPromoCode> UserPromoCodes => Set<UserPromoCode>();
        public DbSet<UserFavoriteLocation> UserFavoriteLocations => Set<UserFavoriteLocation>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<Tariff> Tariffs => Set<Tariff>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserPromoCode>()
                .HasKey(x => new { x.UserId, x.PromoCodeId });

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany(d => d.Rides)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rides)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Tariff)
                .WithMany()
                .HasForeignKey(r => r.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.StartLocation)
                .WithMany(l => l.StartLocationRides)
                .HasForeignKey(r => r.StartLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.EndLocation)
                .WithMany(l => l.EndLocationRides)
                .HasForeignKey(r => r.EndLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Ride)
                .WithOne(r => r.Payment)
                .HasForeignKey<Payment>(p => p.RideId);

            modelBuilder.Entity<DriverStatusHistory>()
                .HasOne(h => h.Driver)
                .WithMany(d => d.StatusHistory)
                .HasForeignKey(h => h.DriverId);

            modelBuilder.Entity<RideStatusHistory>()
                .HasOne(h => h.Ride)
                .WithMany(r => r.StatusHistory)
                .HasForeignKey(h => h.RideId);

            modelBuilder.Entity<UserFavoriteLocation>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.FavoriteLocations)
                .HasForeignKey(uf => uf.UserId);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.FromUser)
                .WithMany(u => u.RatingsGiven)
                .HasForeignKey(r => r.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.ToUser)
                .WithMany(u => u.RatingsReceived)
                .HasForeignKey(r => r.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.ToDriver)
                .WithMany(d => d.RatingsReceived)
                .HasForeignKey(r => r.ToDriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Ride)
                .WithMany(rid => rid.Ratings)
                .HasForeignKey(r => r.RideId);

            modelBuilder.Entity<Tariff>(entity =>
            {
                entity.Property(x => x.BaseFare).HasColumnType("decimal(18,2)");
                entity.Property(x => x.PricePerKm).HasColumnType("decimal(18,2)");
                entity.Property(x => x.PricePerMinute).HasColumnType("decimal(18,2)");
                entity.Property(x => x.MinimumFare).HasColumnType("decimal(18,2)");
                entity.Property(x => x.CancellationFee).HasColumnType("decimal(18,2)");
                entity.Property(x => x.FreeKm).HasColumnType("decimal(18,2)");
                entity.Property(x => x.NightMultiplier).HasColumnType("decimal(18,2)");
                entity.Property(x => x.WaitingPricePerMinute).HasColumnType("decimal(18,2)");

                entity.HasIndex(x => new { x.CityKey, x.CountryCode }).IsUnique();

                var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                // Seed Russian city tariffs
                entity.HasData(
                    new Tariff { Id = 1, CountryCode = "RU", CityKey = "moscow", DisplayCityName = "Москва", BaseFare = 180, PricePerKm = 28, PricePerMinute = 9, MinimumFare = 350, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 2, CountryCode = "RU", CityKey = "saint_petersburg", DisplayCityName = "Санкт-Петербург", BaseFare = 160, PricePerKm = 25, PricePerMinute = 8, MinimumFare = 320, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 3, CountryCode = "RU", CityKey = "yekaterinburg", DisplayCityName = "Екатеринбург", BaseFare = 130, PricePerKm = 22, PricePerMinute = 7, MinimumFare = 280, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 4, CountryCode = "RU", CityKey = "novosibirsk", DisplayCityName = "Новосибирск", BaseFare = 130, PricePerKm = 22, PricePerMinute = 7, MinimumFare = 280, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 5, CountryCode = "RU", CityKey = "kazan", DisplayCityName = "Казань", BaseFare = 120, PricePerKm = 20, PricePerMinute = 6, MinimumFare = 260, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 6, CountryCode = "RU", CityKey = "krasnoyarsk", DisplayCityName = "Красноярск", BaseFare = 120, PricePerKm = 20, PricePerMinute = 6, MinimumFare = 260, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 7, CountryCode = "RU", CityKey = "nizhny_novgorod", DisplayCityName = "Нижний Новгород", BaseFare = 125, PricePerKm = 21, PricePerMinute = 6, MinimumFare = 270, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 8, CountryCode = "RU", CityKey = "chelyabinsk", DisplayCityName = "Челябинск", BaseFare = 120, PricePerKm = 20, PricePerMinute = 6, MinimumFare = 260, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 9, CountryCode = "RU", CityKey = "ufa", DisplayCityName = "Уфа", BaseFare = 115, PricePerKm = 19, PricePerMinute = 6, MinimumFare = 250, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 10, CountryCode = "RU", CityKey = "samara", DisplayCityName = "Самара", BaseFare = 120, PricePerKm = 20, PricePerMinute = 6, MinimumFare = 260, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 11, CountryCode = "RU", CityKey = "rostov_on_don", DisplayCityName = "Ростов-на-Дону", BaseFare = 125, PricePerKm = 21, PricePerMinute = 6, MinimumFare = 270, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 12, CountryCode = "RU", CityKey = "krasnodar", DisplayCityName = "Краснодар", BaseFare = 130, PricePerKm = 22, PricePerMinute = 7, MinimumFare = 280, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 13, CountryCode = "RU", CityKey = "omsk", DisplayCityName = "Омск", BaseFare = 115, PricePerKm = 19, PricePerMinute = 6, MinimumFare = 250, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 14, CountryCode = "RU", CityKey = "voronezh", DisplayCityName = "Воронеж", BaseFare = 120, PricePerKm = 20, PricePerMinute = 6, MinimumFare = 260, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 15, CountryCode = "RU", CityKey = "perm", DisplayCityName = "Пермь", BaseFare = 125, PricePerKm = 21, PricePerMinute = 6, MinimumFare = 270, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 16, CountryCode = "RU", CityKey = "volgograd", DisplayCityName = "Волгоград", BaseFare = 120, PricePerKm = 20, PricePerMinute = 6, MinimumFare = 260, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 17, CountryCode = "RU", CityKey = "tyumen", DisplayCityName = "Тюмень", BaseFare = 130, PricePerKm = 22, PricePerMinute = 7, MinimumFare = 280, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 18, CountryCode = "RU", CityKey = "saratov", DisplayCityName = "Саратов", BaseFare = 115, PricePerKm = 19, PricePerMinute = 6, MinimumFare = 250, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 19, CountryCode = "RU", CityKey = "dagestan_avg", DisplayCityName = "Дагестан (orta)", BaseFare = 110, PricePerKm = 18, PricePerMinute = 5, MinimumFare = 230, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 20, CountryCode = "RU", CityKey = "norilsk", DisplayCityName = "Норильск", BaseFare = 200, PricePerKm = 35, PricePerMinute = 12, MinimumFare = 400, IsActive = true, CreatedTime = seedDate },
                    new Tariff { Id = 21, CountryCode = "RU", CityKey = "default_ru", DisplayCityName = "Russia Default", BaseFare = 120, PricePerKm = 20, PricePerMinute = 6, MinimumFare = 260, IsActive = true, CreatedTime = seedDate }
                );
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<PromoCode>(entity =>
            {
                entity.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
                entity.Property(x => x.MaxDiscountAmount).HasColumnType("decimal(18,2)");
            });
        }
    }
}

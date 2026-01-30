using Microsoft.EntityFrameworkCore;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Domain.Entities;
using TurboTaxi.Infrastructure.Data;
using TurboTaxi.Models.Drivers;
using TurboTaxi.Models.Vehicles;

namespace TurboTaxi.Infrastructure.Services
{
    public class DriverCrudService : IDriverCrudService
    {
        private readonly ApplicationDbContext _db;
        public DriverCrudService(ApplicationDbContext db) => _db = db;

        public async Task<DriverDto> CreateAsync(DriverCreateRequest request, CancellationToken ct)
        {
            var entity = new Driver
            {
                UserId = request.UserId,
                IsVerified = request.IsVerified,
                VehicleId = request.VehicleId
            };
            _db.Drivers.Add(entity);
            await _db.SaveChangesAsync(ct);
            return Map(entity);
        }

        public async Task<DriverDto> UpdateAsync(int id, DriverUpdateRequest request, CancellationToken ct)
        {
            var entity = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (entity is null) throw new KeyNotFoundException("Driver not found");
            if (request.IsVerified.HasValue) entity.IsVerified = request.IsVerified.Value;
            entity.VehicleId = request.VehicleId;
            entity.UpdatedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (entity is null) return false;
            _db.Drivers.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<DriverDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var entity = await _db.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
            return entity is null ? null : Map(entity);
        }

        public async Task<DriverDto?> GetByUserIdAsync(int userId, CancellationToken ct)
        {
            var entity = await _db.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId, ct);
            return entity is null ? null : Map(entity);
        }

        public async Task<DriverDetailDto?> GetDetailByIdAsync(int id, CancellationToken ct)
        {
            var driver = await _db.Drivers
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Vehicle)
                .FirstOrDefaultAsync(d => d.Id == id, ct);

            if (driver is null) return null;

            return new DriverDetailDto
            {
                Id = driver.Id,
                UserId = driver.UserId,
                FullName = driver.User.FullName,
                PhoneNumber = driver.User.PhoneNumber,
                Email = driver.User.Email,
                IsActive = driver.User.IsActive,
                IsVerified = driver.IsVerified,
                VehicleId = driver.VehicleId,
                Vehicle = driver.Vehicle == null ? null : new VehicleDto
                {
                    Id = driver.Vehicle.Id,
                    PlateNumber = driver.Vehicle.PlateNumber,
                    Brand = driver.Vehicle.Brand,
                    Model = driver.Vehicle.Model,
                    Color = driver.Vehicle.Color,
                    Year = driver.Vehicle.Year,
                    VehicleType = driver.Vehicle.VehicleType.ToString(),
                    CreatedAt = driver.Vehicle.CreatedTime
                },
                CreatedAt = driver.CreatedTime,
                UpdatedAt = driver.UpdatedTime
            };
        }

        public async Task<IReadOnlyList<DriverDto>> GetAllAsync(CancellationToken ct)
        {
            return await _db.Drivers.AsNoTracking()
                .OrderByDescending(d => d.Id)
                .Select(d => new DriverDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    IsVerified = d.IsVerified,
                    VehicleId = d.VehicleId,
                    CreatedAt = d.CreatedTime,
                    UpdatedAt = d.UpdatedTime
                })
                .ToListAsync(ct);
        }

        private static DriverDto Map(Driver d) => new()
        {
            Id = d.Id,
            UserId = d.UserId,
            IsVerified = d.IsVerified,
            VehicleId = d.VehicleId,
            CreatedAt = d.CreatedTime,
            UpdatedAt = d.UpdatedTime
        };
    }
}

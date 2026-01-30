using Microsoft.EntityFrameworkCore;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Domain.Enums;
using TurboTaxi.Infrastructure.Data;
using TurboTaxi.Models.Rides;

namespace TurboTaxi.Infrastructure.Services
{
    public class RideQueryService
    {
        private readonly ApplicationDbContext _db;

        public RideQueryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RideDetailResponse> GetByIdAsync(int rideId, int requestingUserId, CancellationToken ct = default)
        {
            var ride = await _db.Rides
                .Include(r => r.User)
                .Include(r => r.Driver).ThenInclude(d => d!.User)
                .Include(r => r.Driver).ThenInclude(d => d!.Vehicle)
                .Include(r => r.Tariff)
                .Include(r => r.StartLocation)
                .Include(r => r.EndLocation)
                .FirstOrDefaultAsync(r => r.Id == rideId, ct);

            if (ride == null)
                throw new KeyNotFoundException($"Ride {rideId} not found");

            // Authorization: user yalnız öz ride-larını görə bilər
            var driverUserId = ride.Driver?.UserId;
            if (ride.UserId != requestingUserId && driverUserId != requestingUserId)
                throw new UnauthorizedAccessException("You don't have access to this ride");

            return MapToRideDetail(ride);
        }

        public async Task<RideDetailResponse?> GetActiveRideAsync(int userId, CancellationToken ct = default)
        {
            var ride = await _db.Rides
                .Include(r => r.User)
                .Include(r => r.Driver).ThenInclude(d => d!.User)
                .Include(r => r.Driver).ThenInclude(d => d!.Vehicle)
                .Include(r => r.Tariff)
                .Include(r => r.StartLocation)
                .Include(r => r.EndLocation)
                .Where(r => r.UserId == userId &&
                       (r.Status == RideStatus.Pending ||
                        r.Status == RideStatus.Approved ||
                        r.Status == RideStatus.Arrived ||
                        r.Status == RideStatus.OnWay))
                .OrderByDescending(r => r.CreatedTime)
                .FirstOrDefaultAsync(ct);

            return ride != null ? MapToRideDetail(ride) : null;
        }

        public async Task<RideHistoryResponse> GetHistoryAsync(int userId, RideHistoryQuery query, CancellationToken ct = default)
        {
            var queryable = _db.Rides
                .Include(r => r.Driver).ThenInclude(d => d!.User)
                .Include(r => r.StartLocation)
                .Include(r => r.EndLocation)
                .Where(r => r.UserId == userId);

            // Filters
            if (query.Status.HasValue)
                queryable = queryable.Where(r => r.Status == query.Status.Value);

            if (query.From.HasValue)
                queryable = queryable.Where(r => r.CreatedTime >= query.From.Value);

            if (query.To.HasValue)
                queryable = queryable.Where(r => r.CreatedTime <= query.To.Value);

            var totalCount = await queryable.CountAsync(ct);

            var rides = await queryable
                .OrderByDescending(r => r.CreatedTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(r => new RideHistoryItemDto
                {
                    Id = r.Id,
                    Status = r.Status,
                    PickupAddress = $"{r.StartLocation!.Latitude:F4}, {r.StartLocation.Longitude:F4}",
                    DropoffAddress = $"{r.EndLocation!.Latitude:F4}, {r.EndLocation.Longitude:F4}",
                    Price = r.ActualFare ?? r.EstimatedFare ?? 0,
                    CreatedAt = r.CreatedTime,
                    FinishedAt = r.CompletedTime,
                    DriverName = r.Driver != null ? r.Driver.User.FullName : null
                })
                .ToListAsync(ct);

            return new RideHistoryResponse
            {
                Rides = rides,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        private RideDetailResponse MapToRideDetail(Domain.Entities.Ride ride)
        {
            return new RideDetailResponse
            {
                Id = ride.Id,
                Status = ride.Status,
                Pickup = new LocationDto(ride.StartLocation!.Latitude, ride.StartLocation.Longitude, null),
                Dropoff = new LocationDto(ride.EndLocation!.Latitude, ride.EndLocation.Longitude, null),
                EstimatedPrice = ride.EstimatedFare ?? 0,
                FinalPrice = ride.ActualFare,
                Tariff = new TariffSnapshotDto
                {
                    CityName = ride.Tariff?.DisplayCityName ?? "Unknown",
                    BaseFare = ride.Tariff?.BaseFare ?? 0,
                    PricePerKm = ride.Tariff?.PricePerKm ?? 0,
                    PricePerMinute = ride.Tariff?.PricePerMinute ?? 0,
                    MinimumFare = ride.Tariff?.MinimumFare ?? 0
                },
                UserId = ride.UserId,
                UserName = ride.User.FullName,
                UserPhone = ride.User.PhoneNumber,
                DriverId = ride.DriverId,
                Driver = ride.Driver != null ? new DriverSummaryDto
                {
                    Id = ride.Driver.Id,
                    Name = ride.Driver.User.FullName,
                    Phone = ride.Driver.User.PhoneNumber,
                    Rating = 4.5,
                    Vehicle = new VehicleDto
                    {
                        Brand = ride.Driver.Vehicle!.Brand,
                        Model = ride.Driver.Vehicle.Model,
                        Color = ride.Driver.Vehicle.Color,
                        PlateNumber = ride.Driver.Vehicle.PlateNumber,
                        Year = ride.Driver.Vehicle.Year
                    }
                } : null,
                CreatedAt = ride.CreatedTime,
                AcceptedAt = ride.AcceptedTime,
                StartedAt = ride.PickUpTime,
                FinishedAt = ride.CompletedTime,
                CanceledAt = ride.CanceledTime,
                CanceledBy = null,
                CancellationReason = null
            };
        }
    }
}

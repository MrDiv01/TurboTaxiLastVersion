using Microsoft.EntityFrameworkCore;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Domain.Enums;
using TurboTaxi.Infrastructure.Data;
using TurboTaxi.Models.Promos;

namespace TurboTaxi.Infrastructure.Services
{
    public class PromoService : IPromoService
    {
        private readonly ApplicationDbContext _db;

        public PromoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CreatePromoResponse> CreateAsync(CreatePromoRequest request, CancellationToken ct = default)
        {
            var code = request.Code?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                return new CreatePromoResponse { Success = false, Message = "Code is required" };
            }

            var exists = await _db.PromoCodes.AnyAsync(p => p.Code.ToUpper() == code, ct);
            if (exists)
            {
                return new CreatePromoResponse { Success = false, Message = "Promo code already exists" };
            }

            var promo = new Domain.Entities.PromoCode
            {
                Code = code,
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                // MaxDiscountAmount no longer used
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MaxUsageCount = request.MaxUsageCount,
                MaxUsagePerUser = request.MaxUsagePerUser,
                IsActive = request.IsActive,
                CreatedTime = DateTime.UtcNow
            };

            _db.PromoCodes.Add(promo);
            await _db.SaveChangesAsync(ct);

            // If user-specific, create mapping so only that user can use
            if (request.UserId.HasValue && request.UserId.Value > 0)
            {
                var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId.Value, ct);
                if (!userExists)
                {
                    return new CreatePromoResponse { Success = false, Message = "User not found" };
                }

                _db.UserPromoCodes.Add(new Domain.Entities.UserPromoCode
                {
                    UserId = request.UserId.Value,
                    PromoCodeId = promo.Id,
                    UsedCount = 0,
                    LastUsedTime = null
                });
                await _db.SaveChangesAsync(ct);
            }

            return new CreatePromoResponse
            {
                Success = true,
                PromoCodeId = promo.Id,
                Message = "Promo code created"
            };
        }

        public async Task<ApplyPromoResponse> ApplyAsync(ApplyPromoRequest request, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var code = request.Code?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(code) || request.UserId <= 0 || request.Subtotal <= 0)
            {
                return new ApplyPromoResponse
                {
                    Success = false,
                    Message = "Invalid promo request",
                    Subtotal = request.Subtotal,
                    Discount = 0,
                    FinalTotal = request.Subtotal
                };
            }

            var promo = await _db.PromoCodes.FirstOrDefaultAsync(p => p.Code.ToUpper() == code, ct);
            if (promo == null || !promo.IsActive)
            {
                return Fail("Promo code tapılmadı və ya deaktivdir", request.Subtotal);
            }

            if (promo.StartDate.HasValue && now < promo.StartDate.Value)
            {
                return Fail("Promo hələ aktiv deyil", request.Subtotal);
            }

            if (promo.EndDate.HasValue && now > promo.EndDate.Value)
            {
                return Fail("Promo müddəti bitib", request.Subtotal);
            }

            // Global usage check: count rides using this promo
            if (promo.MaxUsageCount.HasValue)
            {
                var usedCount = await _db.Rides.CountAsync(r => r.PromoCodeId == promo.Id, ct);
                if (usedCount >= promo.MaxUsageCount.Value)
                {
                    return Fail("Promo artıq istifadə limitini keçib", request.Subtotal);
                }
            }

            // Per-user usage check. If promo has specific user bindings, only those users can use it.
            var userPromo = await _db.UserPromoCodes.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.PromoCodeId == promo.Id, ct);

            // If there is any mapping for this promo, require the current user to have one
            var hasMappings = await _db.UserPromoCodes.AnyAsync(x => x.PromoCodeId == promo.Id, ct);
            if (hasMappings && userPromo == null)
            {
                return Fail("Bu promo kod yalnız müəyyən istifadəçi üçün nəzərdə tutulub", request.Subtotal);
            }

            if (promo.MaxUsagePerUser.HasValue && userPromo?.UsedCount >= promo.MaxUsagePerUser.Value)
            {
                return Fail("Bu promo artıq istifadə olunub", request.Subtotal);
            }

            // Calculate discount
            decimal discount;
            if (promo.DiscountType == PromoDiscountType.FixedAmount)
            {
                discount = Math.Min(promo.DiscountValue, request.Subtotal);
            }
            else // percentage: apply exact rate
            {
                discount = request.Subtotal * (promo.DiscountValue / 100m);
            }

            var finalTotal = Math.Max(0, request.Subtotal - discount);

            if (request.Consume)
            {
                // Persist per-user usage increment
                if (userPromo == null)
                {
                    userPromo = new Domain.Entities.UserPromoCode
                    {
                        UserId = request.UserId,
                        PromoCodeId = promo.Id,
                        UsedCount = 1,
                        LastUsedTime = now
                    };
                    _db.UserPromoCodes.Add(userPromo);
                }
                else
                {
                    userPromo.UsedCount += 1;
                    userPromo.LastUsedTime = now;
                    _db.UserPromoCodes.Update(userPromo);
                }

                await _db.SaveChangesAsync(ct);
            }

            return new ApplyPromoResponse
            {
                Success = true,
                Message = "Promo tətbiq olundu",
                Subtotal = request.Subtotal,
                Discount = discount,
                FinalTotal = finalTotal,
                PromoCodeId = promo.Id,
                PromoCode = promo.Code,
                Consumed = request.Consume
            };
        }

        private ApplyPromoResponse Fail(string message, decimal subtotal) => new ApplyPromoResponse
        {
            Success = false,
            Message = message,
            Subtotal = subtotal,
            Discount = 0,
            FinalTotal = subtotal
        };
    }
}

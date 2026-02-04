using TurboTaxi.Domain.Enums;

namespace TurboTaxi.Models.Promos
{
    public class ApplyPromoRequest
    {
        public string Code { get; set; } = null!;
        public int UserId { get; set; }
        public decimal Subtotal { get; set; }

        /// <summary>
        /// If true, usage limits are consumed and saved; if false, just calculate preview.
        /// </summary>
        public bool Consume { get; set; } = false;
    }

    public class ApplyPromoResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalTotal { get; set; }

        public int? PromoCodeId { get; set; }
        public string? PromoCode { get; set; }
        public bool Consumed { get; set; }
    }

    public class CreatePromoRequest
    {
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        // FixedAmount (məbleg) | Percentage (faiz)
        public PromoDiscountType DiscountType { get; set; } = PromoDiscountType.FixedAmount;
        public decimal DiscountValue { get; set; }
        // MaxDiscountAmount removed: percentage discounts apply exactly by DiscountValue
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxUsageCount { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Əgər doludursa promo yalnız bu user üçündür. Boşdursa hamı istifadə edə bilər.
        /// </summary>
        public int? UserId { get; set; }
    }

    public class CreatePromoResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int PromoCodeId { get; set; }
    }
}

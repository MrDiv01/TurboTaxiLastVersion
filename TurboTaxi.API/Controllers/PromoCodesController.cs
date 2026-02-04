using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Models.Promos;

namespace TurboTaxi.API.Controllers
{
    [ApiController]
    [Route("api/promocodes")]
    public class PromoCodesController : ControllerBase
    {
        private readonly IPromoService _promoService;

        public PromoCodesController(IPromoService promoService)
        {
            _promoService = promoService;
        }

        /// <summary>
        /// Create a promo code (admin use). DiscountType: FixedAmount or Percentage
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<CreatePromoResponse>> CreatePromo([FromBody] CreatePromoRequest request, CancellationToken ct)
        {
            var result = await _promoService.CreateAsync(request, ct);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Promo kodu tətbiq edir, endirimi hesablayır və istifadə sayını artırır.
        /// </summary>
        [HttpPost("apply")]
        [Authorize]
        public async Task<ActionResult<ApplyPromoResponse>> Apply([FromBody] ApplyPromoRequest request, CancellationToken ct)
        {
            // UserId-i token-dən də götürə bilərik, amma hazırda request-dən oxuyuruq
            var result = await _promoService.ApplyAsync(request, ct);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}

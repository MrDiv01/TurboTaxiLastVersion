using TurboTaxi.Models.Promos;

namespace TurboTaxi.Application.Interfaces
{
    public interface IPromoService
    {
        Task<ApplyPromoResponse> ApplyAsync(ApplyPromoRequest request, CancellationToken ct = default);
        Task<CreatePromoResponse> CreateAsync(CreatePromoRequest request, CancellationToken ct = default);
    }
}

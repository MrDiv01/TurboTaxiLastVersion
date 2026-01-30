using MediatR;
using TurboTaxi.Models.Drivers;

namespace TurboTaxi.Application.CQRS.Drivers.Queries.GetDetailById
{
    public sealed record GetDriverDetailQuery(int DriverId) : IRequest<DriverDetailDto>;
}

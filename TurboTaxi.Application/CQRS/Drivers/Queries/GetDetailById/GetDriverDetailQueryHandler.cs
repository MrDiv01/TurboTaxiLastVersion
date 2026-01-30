using MediatR;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Models.Drivers;

namespace TurboTaxi.Application.CQRS.Drivers.Queries.GetDetailById
{
    public sealed class GetDriverDetailQueryHandler : IRequestHandler<GetDriverDetailQuery, DriverDetailDto>
    {
        private readonly IDriverCrudService _service;

        public GetDriverDetailQueryHandler(IDriverCrudService service)
        {
            _service = service;
        }

        public async Task<DriverDetailDto> Handle(GetDriverDetailQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetDetailByIdAsync(request.DriverId, cancellationToken);

            if (result is null)
                throw new KeyNotFoundException($"Driver with ID {request.DriverId} not found");

            return result;
        }
    }
}

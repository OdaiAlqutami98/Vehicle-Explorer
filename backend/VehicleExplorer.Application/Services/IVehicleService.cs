using VehicleExplorer.Application.DTOs;
using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Application.Services;

public interface IVehicleService
{
    Task<IReadOnlyList<Make>> GetMakesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<VehicleType>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken);
    Task<IReadOnlyList<VehicleModel>> GetModelsAsync(GetModelsRequest request, CancellationToken cancellationToken);
}

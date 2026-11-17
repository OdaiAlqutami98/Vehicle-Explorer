using Microsoft.AspNetCore.Mvc;
using VehicleExplorer.Application.DTOs;
using VehicleExplorer.Application.Models;
using VehicleExplorer.Application.Services;

namespace VehicleExplorer.Api.Controllers;

[ApiController]
[Route("api/makes")]
public class MakesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpGet("GetMakes")]
    public Task<IReadOnlyList<Make>> GetMakes(CancellationToken cancellationToken)
    {
        return vehicleService.GetMakesAsync(cancellationToken);
    }

    [HttpGet("GetVehicleTypes")]
    public Task<IReadOnlyList<VehicleType>> GetVehicleTypes(int makeId, CancellationToken cancellationToken)
    {
        return vehicleService.GetVehicleTypesAsync(makeId, cancellationToken);
    }

    [HttpGet("GetModels")]
    public Task<IReadOnlyList<VehicleModel>> GetModels([FromQuery] GetModelsRequest request, CancellationToken cancellationToken)
    {
        return vehicleService.GetModelsAsync(request, cancellationToken);
    }
}

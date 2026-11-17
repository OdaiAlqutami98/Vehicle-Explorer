using VehicleExplorer.Application.DTOs;
using VehicleExplorer.Application.Exceptions;
using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Application.Services;

public class VehicleService(IVehicleDataProvider dataProvider, TimeProvider timeProvider) : IVehicleService
{
    public const int FirstSupportedYear = 1995;

    public async Task<IReadOnlyList<Make>> GetMakesAsync(CancellationToken cancellationToken)
    {
        var makes = await dataProvider.GetMakesAsync(cancellationToken);

        return makes.OrderBy(make => make.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyList<VehicleType>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken)
    {
        ValidateMakeId(makeId);

        var vehicleTypes = await dataProvider.GetVehicleTypesAsync(makeId, cancellationToken);

        return vehicleTypes.OrderBy(vehicleType => vehicleType.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyList<VehicleModel>> GetModelsAsync(GetModelsRequest request, CancellationToken cancellationToken)
    {
        ValidateMakeId(request.MakeId);
        ValidateYear(request.Year);

        
        var normalizedRequest = new GetModelsRequest
        {
            MakeId = request.MakeId,
            Year = request.Year,
            VehicleType = string.IsNullOrWhiteSpace(request.VehicleType) ? null : request.VehicleType.Trim(),
        };

        var models = await dataProvider.GetModelsAsync(normalizedRequest, cancellationToken);

        return models.OrderBy(model => model.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static void ValidateMakeId(int makeId)
    {
        if (makeId <= 0)
        {
            throw new InvalidRequestException("Make id must be a positive number.");
        }
    }

    private void ValidateYear(int year)
    {
        var lastSupportedYear = timeProvider.GetUtcNow().Year + 1;

        if (year < FirstSupportedYear || year > lastSupportedYear)
        {
            throw new InvalidRequestException(
                $"Year must be between {FirstSupportedYear} and {lastSupportedYear}.");
        }
    }
}

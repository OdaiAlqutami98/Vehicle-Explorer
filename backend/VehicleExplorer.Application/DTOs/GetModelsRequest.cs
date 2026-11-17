namespace VehicleExplorer.Application.DTOs;

public class GetModelsRequest
{
    public int MakeId { get; init; }

    public int Year { get; init; }

    public string? VehicleType { get; init; }
}

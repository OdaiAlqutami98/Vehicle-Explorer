using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Nhtsa.Contracts;

internal class NhtsaVehicleType
{
    [JsonPropertyName("VehicleTypeId")]
    public int VehicleTypeId { get; set; }

    [JsonPropertyName("VehicleTypeName")]
    public string? VehicleTypeName { get; set; }
}

using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Nhtsa.Contracts;

internal class NhtsaModel
{
    [JsonPropertyName("Model_ID")]
    public int ModelId { get; set; }

    [JsonPropertyName("Model_Name")]
    public string? ModelName { get; set; }

    [JsonPropertyName("VehicleTypeName")]
    public string? VehicleTypeName { get; set; }
}

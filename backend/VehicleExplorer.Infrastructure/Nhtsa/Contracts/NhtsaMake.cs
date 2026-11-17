using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Nhtsa.Contracts;

internal class NhtsaMake
{
    [JsonPropertyName("Make_ID")]
    public int MakeId { get; set; }

    [JsonPropertyName("Make_Name")]
    public string? MakeName { get; set; }
}

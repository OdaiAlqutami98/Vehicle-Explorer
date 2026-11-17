namespace VehicleExplorer.Infrastructure.Nhtsa;

public class NhtsaOptions
{
    public const string SectionName = "Nhtsa";

    public string BaseUrl { get; set; } = string.Empty;

    public int CacheHours { get; set; } = 24;
}

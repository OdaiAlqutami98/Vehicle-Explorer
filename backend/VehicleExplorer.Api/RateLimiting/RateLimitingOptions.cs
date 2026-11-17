namespace VehicleExplorer.Api.RateLimiting;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public const string PerIpPolicyName = "per-ip";
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
}

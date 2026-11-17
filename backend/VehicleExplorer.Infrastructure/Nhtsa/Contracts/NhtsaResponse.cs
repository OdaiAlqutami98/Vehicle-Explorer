namespace VehicleExplorer.Infrastructure.Nhtsa.Contracts;

internal class NhtsaResponse<TResult>
{
    public List<TResult>? Results { get; set; }
}

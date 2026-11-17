using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VehicleExplorer.Application.Services;
using VehicleExplorer.Infrastructure.Nhtsa;

namespace VehicleExplorer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<NhtsaOptions>()
            .Bind(configuration.GetSection(NhtsaOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "Nhtsa:BaseUrl must be an absolute URL.")
            .Validate(options => options.BaseUrl.EndsWith('/'), "Nhtsa:BaseUrl must end with '/'.")
            .Validate(options => options.CacheHours > 0, "Nhtsa:CacheHours must be greater than 0.")
            .ValidateOnStart();

        services.AddMemoryCache();

        services
            .AddHttpClient<IVehicleDataProvider, NhtsaVehicleDataProvider>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<NhtsaOptions>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddStandardResilienceHandler(resilience =>
            {
                resilience.AttemptTimeout.Timeout = TimeSpan.FromSeconds(20);
                resilience.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                resilience.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(40);
            });

        return services;
    }
}

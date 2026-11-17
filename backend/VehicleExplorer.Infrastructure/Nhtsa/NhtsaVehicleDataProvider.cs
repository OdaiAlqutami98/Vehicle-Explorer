using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;
using VehicleExplorer.Application.DTOs;
using VehicleExplorer.Application.Exceptions;
using VehicleExplorer.Application.Models;
using VehicleExplorer.Application.Services;
using VehicleExplorer.Infrastructure.Nhtsa.Contracts;

namespace VehicleExplorer.Infrastructure.Nhtsa;

public class NhtsaVehicleDataProvider(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<NhtsaOptions> options) : IVehicleDataProvider
{
    public Task<IReadOnlyList<Make>> GetMakesAsync(CancellationToken cancellationToken)
    {
        return GetCachedAsync<NhtsaMake, Make>("getallmakes?format=json", ToMake, cancellationToken);
    }

    public Task<IReadOnlyList<VehicleType>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken)
    {
        var relativeUrl = $"GetVehicleTypesForMakeId/{makeId}?format=json";

        return GetCachedAsync<NhtsaVehicleType, VehicleType>(relativeUrl, ToVehicleType, cancellationToken);
    }

    public Task<IReadOnlyList<VehicleModel>> GetModelsAsync(GetModelsRequest request, CancellationToken cancellationToken)
    {
        var path = $"GetModelsForMakeIdYear/makeId/{request.MakeId}/modelyear/{request.Year}";

        if (request.VehicleType is not null)
        {
            path += $"/vehicletype/{Uri.EscapeDataString(request.VehicleType)}";
        }

        return GetCachedAsync<NhtsaModel, VehicleModel>($"{path}?format=json", ToVehicleModel, cancellationToken);
    }

    private async Task<IReadOnlyList<TResult>> GetCachedAsync<TNhtsaItem, TResult>(
        string relativeUrl,
        Func<TNhtsaItem, TResult> map,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"nhtsa:{relativeUrl}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<TResult>? cachedResults) && cachedResults is not null)
        {
            return cachedResults;
        }

        var nhtsaItems = await GetResultsAsync<TNhtsaItem>(relativeUrl, cancellationToken);
        IReadOnlyList<TResult> results = nhtsaItems.Select(map).ToList().AsReadOnly();

        cache.Set(cacheKey, results, TimeSpan.FromHours(options.Value.CacheHours));

        return results;
    }

    private async Task<List<TNhtsaItem>> GetResultsAsync<TNhtsaItem>(
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(relativeUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(
                    $"NHTSA returned {(int)response.StatusCode} ({response.ReasonPhrase}) for '{relativeUrl}'.");
            }

            var result = await response.Content.ReadFromJsonAsync<NhtsaResponse<TNhtsaItem>>(cancellationToken);

            if (result?.Results is null)
            {
                throw new ExternalServiceException($"NHTSA returned no Results for '{relativeUrl}'.");
            }

            return result.Results;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TimeoutRejectedException exception)
        {
            throw new ExternalServiceException(
                $"NHTSA request timed out for '{relativeUrl}'.",
                exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw new ExternalServiceException(
                $"NHTSA circuit breaker is open for '{relativeUrl}'.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ExternalServiceException(
                $"An HTTP error occurred while calling NHTSA for '{relativeUrl}'.",
                exception);
        }
        catch (OperationCanceledException exception)
        {
            // Timeout from HttpClient or another cancellation not initiated by the caller.
            throw new ExternalServiceException(
                $"NHTSA request was cancelled or timed out for '{relativeUrl}'.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new ExternalServiceException(
                $"NHTSA returned invalid JSON for '{relativeUrl}'.",
                exception);
        }
    }

    private static Make ToMake(NhtsaMake make)
    {
        return new Make(make.MakeId, Clean(make.MakeName));
    }

    private static VehicleType ToVehicleType(NhtsaVehicleType vehicleType)
    {
        return new VehicleType(vehicleType.VehicleTypeId, Clean(vehicleType.VehicleTypeName));
    }

    private static VehicleModel ToVehicleModel(NhtsaModel model)
    {
        return new VehicleModel(model.ModelId, Clean(model.ModelName), model.VehicleTypeName?.Trim());
    }

    private static string Clean(string? name)
    {
        return name?.Trim() ?? string.Empty;
    }
}

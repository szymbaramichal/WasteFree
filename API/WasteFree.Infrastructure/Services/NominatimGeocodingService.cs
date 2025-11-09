using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure.Options;

namespace WasteFree.Infrastructure.Services;

/// <summary>
/// Geocoding service backed by the OpenStreetMap Nominatim API.
/// </summary>
public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<NominatimGeocodingService> logger;
    private readonly NominatimOptions options;

    public NominatimGeocodingService(
        HttpClient httpClient,
        IOptions<NominatimOptions> options,
        ILogger<NominatimGeocodingService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.options = options.Value;

        if (this.httpClient.BaseAddress is null && !string.IsNullOrWhiteSpace(this.options.BaseUrl))
        {
            this.httpClient.BaseAddress = new Uri(this.options.BaseUrl);
        }
    }

    public async Task<(double Latitude, double Longitude)?> TryGetCoordinatesAsync(Address address, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (string.IsNullOrWhiteSpace(address.Street) ||
            string.IsNullOrWhiteSpace(address.PostalCode) ||
            string.IsNullOrWhiteSpace(address.City))
        {
            return null;
        }

        var query = BuildQuery(address);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"search?{query}", cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error while calling Nominatim geocoding for {Street}, {PostalCode}, {City}",
                address.Street,
                address.PostalCode,
                address.City);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Nominatim geocoding returned status {StatusCode} for {Street}, {PostalCode}, {City}",
                response.StatusCode,
                address.Street,
                address.PostalCode,
                address.City);
            return null;
        }

        IReadOnlyList<NominatimSearchResponse>? results;
        try
        {
            results = await response.Content.ReadFromJsonAsync<IReadOnlyList<NominatimSearchResponse>>(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize Nominatim response for {Street}, {PostalCode}, {City}",
                address.Street,
                address.PostalCode,
                address.City);
            return null;
        }

        var first = results?.FirstOrDefault();
        if (first is null)
        {
            logger.LogWarning("Nominatim returned no results for {Street}, {PostalCode}, {City}",
                address.Street,
                address.PostalCode,
                address.City);
            return null;
        }

        if (!double.TryParse(first.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(first.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            logger.LogWarning("Nominatim returned invalid coordinates ({Lat}, {Lon}) for {Street}, {PostalCode}, {City}",
                first.Lat,
                first.Lon,
                address.Street,
                address.PostalCode,
                address.City);
            return null;
        }

        logger.LogDebug("Resolved coordinates ({Latitude}, {Longitude}) for {Street}, {PostalCode}, {City}",
            latitude,
            longitude,
            address.Street,
            address.PostalCode,
            address.City);

        return (latitude, longitude);
    }

    private static string BuildQuery(Address address)
    {
        var addressText = Uri.EscapeDataString($"{address}");
        var parameters = new Dictionary<string, string>
        {
            ["q"] = addressText,
            ["format"] = "json",
        };  

        return string.Join('&', parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
    }

    private sealed record NominatimSearchResponse(
        [property: JsonPropertyName("lat")] string Lat,
        [property: JsonPropertyName("lon")] string Lon);
}

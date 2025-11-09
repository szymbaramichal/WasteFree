namespace WasteFree.Infrastructure.Options;

/// <summary>
/// Configuration settings for the Nominatim geocoding API.
/// </summary>
public class NominatimOptions
{
    /// <summary>
    /// Base URL of the Nominatim endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://nominatim.openstreetmap.org/";

    /// <summary>
    /// User agent header required by OpenStreetMap usage policy.
    /// </summary>
    public string UserAgent { get; set; } = "WasteFree/1.0 (+https://wastefreecloud.pl)";

    /// <summary>
    /// Optional contact email sent as the From header when defined.
    /// </summary>
    public string? ContactEmail { get; set; }
}

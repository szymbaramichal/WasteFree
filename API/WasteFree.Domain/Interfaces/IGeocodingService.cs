using WasteFree.Domain.Models;

namespace WasteFree.Domain.Interfaces;

public interface IGeocodingService
{
    Task<(double Latitude, double Longitude)?> TryGetCoordinatesAsync(Address address, CancellationToken cancellationToken = default);
}

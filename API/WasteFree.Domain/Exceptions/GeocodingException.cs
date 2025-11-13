using WasteFree.Domain.Models;

namespace WasteFree.Domain.Exceptions;

public sealed class GeocodingException(Address address, Exception innerException)
    : Exception("Unable to resolve coordinates for the specified address.", innerException)
{
    public Address Address { get; } = address;
}

using System.ComponentModel.DataAnnotations;

namespace WasteFree.Domain.Models;

public class Address
{
    /// <summary>
    /// City of location
    /// </summary>
    [MaxLength(100)] public string City { get; set; } = null!;

    /// <summary>
    /// PostalCode of location
    /// </summary>
    [MaxLength(6)] public string PostalCode { get; set; } = null!;

    /// <summary>
    /// Street of location
    /// </summary>
    [MaxLength(100)] public string Street { get; set; } = null!;

    /// <summary>
    /// Latitude coordinate resolved via geocoding service
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate resolved via geocoding service
    /// </summary>
    public double? Longitude { get; set; }
    
    public override string ToString()
    {
        return $"{Street}, {PostalCode} {City}";
    }
}
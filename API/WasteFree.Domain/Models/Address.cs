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
}
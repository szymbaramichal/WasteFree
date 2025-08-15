using System.ComponentModel.DataAnnotations;
using WasteFree.Shared.Shared;

namespace WasteFree.Shared.Entities;

public class User : DatabaseEntity
{
    [MaxLength(100)]
    public required string Username { get; set; }
    
    [MaxLength(100)]
    public required string Email { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public required byte[] PasswordHash { get; set; }

    public required byte[] PasswordSalt { get; set; }
}
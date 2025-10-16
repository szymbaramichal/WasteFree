using System.ComponentModel.DataAnnotations;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

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
    
    public bool IsActive { get; set; } = false;
    
    public required LanguagePreference LanguagePreference { get; set; }
    
    public required UserRole Role { get; set; }

    public string? AvatarName { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    public ICollection<UserGarbageGroup> UserGarbageGroups { get; set; } = new List<UserGarbageGroup>();
    public ICollection<InboxNotification> InboxNotifications { get; set; } = new List<InboxNotification>();

    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
}
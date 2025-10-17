using System.ComponentModel.DataAnnotations;
using WasteFree.Domain.Models;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

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
    
    public bool ConsentsAgreed { get; set; } = true;
    
    public required LanguagePreference LanguagePreference { get; set; }
    
    public required UserRole Role { get; set; }

    public string? AvatarName { get; set; }
    
    public required Address Address { get; set; } = new();
    
    public ICollection<UserGarbageGroup> UserGarbageGroups { get; set; } = new List<UserGarbageGroup>();
    public ICollection<InboxNotification> InboxNotifications { get; set; } = new List<InboxNotification>();

    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
}
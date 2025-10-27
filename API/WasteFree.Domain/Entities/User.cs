using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
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

    public string PickupOptions { get; set; } = string.Empty;
    
    public required Address Address { get; set; } = new();
    
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
    
    public ICollection<UserGarbageGroup> UserGarbageGroups { get; set; } = new List<UserGarbageGroup>();
    public ICollection<InboxNotification> InboxNotifications { get; set; } = new List<InboxNotification>();

    [NotMapped]
    public PickupOption[]? PickupOptionsList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PickupOptions)) return [];

            var tokens = PickupOptions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var parsed = new List<PickupOption>(tokens.Length);

            foreach (var token in tokens)
            {
                if (Enum.TryParse<PickupOption>(token, true, out var option))
                {
                    parsed.Add(option);
                    continue;
                }

                if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric) &&
                    Enum.IsDefined(typeof(PickupOption), numeric))
                {
                    parsed.Add((PickupOption)numeric);
                }
            }

            return parsed.Distinct().ToArray();
        }
        set
        {
            if (value.Length == 0)
            {
                PickupOptions = string.Empty;
                return;
            }

            var normalized = value.Distinct().Select(v => ((int)v).ToString(CultureInfo.InvariantCulture));
            PickupOptions = string.Join(',', normalized);
        }
    }
}
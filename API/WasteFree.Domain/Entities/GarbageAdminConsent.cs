using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class GarbageAdminConsent : DatabaseEntity
{
    public required string Content { get; set; }
    public required LanguagePreference Language { get; set; }
}
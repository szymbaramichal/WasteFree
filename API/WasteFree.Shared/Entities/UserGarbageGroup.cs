using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Shared.Entities;

public class UserGarbageGroup : DatabaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GarbageGroupId { get; set; }
    public GarbageGroup GarbageGroup { get; set; } = null!;

    public GarbageGroupRole Role { get; set; }
}
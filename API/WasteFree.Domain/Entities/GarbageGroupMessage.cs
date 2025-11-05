using System.ComponentModel.DataAnnotations;
using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class GarbageGroupMessage : DatabaseEntity
{
    public Guid GarbageGroupId { get; set; }
    public GarbageGroup GarbageGroup { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(2000)]
    public required string Content { get; set; }
}

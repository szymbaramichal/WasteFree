using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities
{
    public class GarbageGroup : DatabaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        
        public required Address Address { get; set; } = new();
        public bool IsPrivate { get; set; }

        public ICollection<UserGarbageGroup> UserGarbageGroups { get; set; } = new List<UserGarbageGroup>();
        public ICollection<GarbageGroupMessage> Messages { get; set; } = new List<GarbageGroupMessage>();
    }
}
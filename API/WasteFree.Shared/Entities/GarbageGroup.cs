using WasteFree.Shared.Models;

namespace WasteFree.Shared.Entities
{
    public class GarbageGroup : DatabaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }

        public ICollection<UserGarbageGroup> UserGarbageGroups { get; set; } = new List<UserGarbageGroup>();
    }
}
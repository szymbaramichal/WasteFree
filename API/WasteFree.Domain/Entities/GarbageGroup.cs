using WasteFree.Shared.Models;

namespace WasteFree.Shared.Entities
{
    public class GarbageGroup : DatabaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string City { get; set; }
        public required string PostalCode { get; set; }
        public required string Address { get; set; }
        public bool IsPrivate { get; set; }

        public ICollection<UserGarbageGroup> UserGarbageGroups { get; set; } = new List<UserGarbageGroup>();
    }
}
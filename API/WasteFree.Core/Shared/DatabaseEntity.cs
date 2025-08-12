using System.ComponentModel.DataAnnotations;

namespace WasteFree.Shared.Shared;

public class DatabaseEntity
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public int CreatedBy { get; set; }
    public DateTime ModifiedDateUtc { get; set; }
    public int ModifiedBy { get; set; }
}
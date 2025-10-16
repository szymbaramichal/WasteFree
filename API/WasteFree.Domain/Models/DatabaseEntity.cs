using System.ComponentModel.DataAnnotations;

namespace WasteFree.Shared.Models;

public class DatabaseEntity
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? ModifiedDateUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
}
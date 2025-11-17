using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class GarbageOrder : DatabaseEntity
{
    public PickupOption PickupOption { get; set; }
    
    public ContainerSize? ContainerSize { get; set; }
    public DateTime? DropOffDate { get; set; }
    
    public DateTime PickupDate { get; set; }

    public bool IsHighPriority { get; set; } = false;
    public bool CollectingService { get; set; } = false;
    
    public GarbageOrderStatus GarbageOrderStatus { get; set; }

    public decimal Cost { get; set; }
    public decimal PrepaidUtilizationFeeAmount { get; set; }

    public GarbageGroup GarbageGroup { get; set; } = null!;
    public Guid GarbageGroupId { get; set; }

    public Guid? AssignedGarbageAdminId { get; set; }
    public User? AssignedGarbageAdmin { get; set; }

    public decimal? UtilizationFeeAmount { get; set; }
    public decimal? AdditionalUtilizationFeeAmount { get; set; }
    public string? UtilizationProofBlobName { get; set; }
    public DateTime? UtilizationFeeSubmittedDateUtc { get; set; }
    
    public ICollection<GarbageOrderUsers> GarbageOrderUsers { get; set; } = [];
}
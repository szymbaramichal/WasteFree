using WasteFree.Domain.Enums;

namespace WasteFree.Application.Services.GarbageOrders;

public interface IGarbageOrderCostCalculator
{
    GarbageOrderCostBreakdown CalculateEstimate(
        PickupOption pickupOption,
        ContainerSize? containerSize,
        DateTime? dropOffDate,
        DateTime pickupDate,
        bool isHighPriority,
        bool collectingService);
}

public class GarbageOrderCostCalculator : IGarbageOrderCostCalculator
{
    private const decimal SmallPickupBase = 50m;
    private const decimal PickupBase = 80m;
    private const decimal ContainerBase = 120m;
    private const decimal SpecialOrderBase = 200m;
    private const decimal CollectingServiceFee = 35m;
    private const decimal HighPriorityMultiplier = 1.25m;
    private const decimal ContainerDailyRate = 15m;
    private const decimal UtilizationFeeMultiplier = 1.25m;

    public GarbageOrderCostBreakdown CalculateEstimate(
        PickupOption pickupOption,
        ContainerSize? containerSize,
        DateTime? dropOffDate,
        DateTime pickupDate,
        bool isHighPriority,
        bool collectingService)
    {
        var estimate = pickupOption switch
        {
            PickupOption.SmallPickup => SmallPickupBase,
            PickupOption.Pickup => PickupBase,
            PickupOption.Container => ContainerBase + ResolveContainerSizeFee(containerSize),
            PickupOption.SpecialOrder => SpecialOrderBase,
            _ => SmallPickupBase
        };

        if (pickupOption == PickupOption.Container)
        {
            var rentalDays = CalculateRentalDays(dropOffDate, pickupDate);
            estimate += rentalDays * ContainerDailyRate;
        }

        if (collectingService)
        {
            estimate += CollectingServiceFee;
        }

        if (isHighPriority)
        {
            estimate *= HighPriorityMultiplier;
        }

        var roundedBaseEstimate = decimal.Round(estimate, 2, MidpointRounding.AwayFromZero);
        var totalWithUtilization = decimal.Round(
            roundedBaseEstimate * UtilizationFeeMultiplier,
            2,
            MidpointRounding.AwayFromZero);
        var prepaidUtilizationFee = totalWithUtilization - roundedBaseEstimate;

        return new GarbageOrderCostBreakdown(
            roundedBaseEstimate,
            prepaidUtilizationFee,
            totalWithUtilization);
    }

    private static decimal ResolveContainerSizeFee(ContainerSize? containerSize) => containerSize switch
    {
        ContainerSize.ContainerSmall => 40m,
        ContainerSize.ContainerMedium => 60m,
        ContainerSize.ContainerLarge => 90m,
        _ => 0m
    };

    private static int CalculateRentalDays(DateTime? dropOffDate, DateTime pickupDate)
    {
        if (!dropOffDate.HasValue)
        {
            return 0;
        }

        var duration = (pickupDate.Date - dropOffDate.Value.Date).Days;
        return Math.Max(duration, 0);
    }
}

public record GarbageOrderCostBreakdown(
    decimal BaseCost,
    decimal PrepaidUtilizationFee,
    decimal TotalCost);

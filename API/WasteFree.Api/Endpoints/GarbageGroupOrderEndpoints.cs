using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Filters;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageGroupOrders;
using WasteFree.Application.Features.GarbageGroupOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Endpoints;

public static class GarbageGroupOrderEndpoints
{
    public static void MapGarbageGroupOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/garbage-group/{groupId:guid}/order", CreateGarbageOrderAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .AddEndpointFilter(new ValidationFilter<GarbageOrderRequest>())
            .WithOpenApi()
            .Produces<Result<GarbageGroupOrderDto>>()
            .Produces<Dictionary<string, string[]>>(422)
            .Produces<Result<EmptyResult>>(400)
            .WithTags("GarbageOrders")
            .WithDescription("Create a new garbage order.");

        app.MapPost("/garbage-group/{groupId:guid}/order/calculate", CalculateGarbageOrderCostAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .AddEndpointFilter(new ValidationFilter<GarbageOrderCalculationRequest>())
            .WithOpenApi()
            .Produces<Result<GarbageOrderCostDto>>()
            .Produces<Dictionary<string, string[]>>(422)
            .Produces<Result<EmptyResult>>(400)
            .WithTags("GarbageOrders")
            .WithDescription("Calculate estimated cost for a garbage order.");
        
        app.MapPost("/garbage-group/{groupId:guid}/orders/filter", GetGarbageOrdersAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .WithOpenApi()
            .Produces<Result<ICollection<GarbageGroupOrderDto>>>()
            .WithTags("GarbageOrders")
            .WithDescription("Get garbage orders for the group.");
    }

    /// <summary>
    /// Create garbage order for a specific garbage group.
    /// </summary>
    private static async Task<IResult> CreateGarbageOrderAsync(
        [FromRoute] Guid groupId,
        [FromBody] GarbageOrderRequest request,
        ICurrentUserService currentUserService,
        IMediator mediator,
        IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        var command = new GarbageGroupOrderCommand(
            groupId,
            currentUserService.UserId,
            request.PickupOption,
            request.UserIds,
            request.ContainerSize,
            request.DropOffDate,
            request.PickupDate,
            request.IsHighPriority,
            request.CollectingService);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = stringLocalizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);    
    }

    /// <summary>
    /// Calculate estimated cost for a garbage order without persisting it.
    /// </summary>
    private static async Task<IResult> CalculateGarbageOrderCostAsync(
        [FromRoute] Guid groupId,
        [FromBody] GarbageOrderCalculationRequest request,
        ICurrentUserService currentUserService,
        IMediator mediator,
        IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        var query = new CalculateGarbageOrderCostQuery(
            groupId,
            currentUserService.UserId,
            request.PickupOption,
            request.ContainerSize,
            request.DropOffDate,
            request.PickupDate,
            request.IsHighPriority,
            request.CollectingService);

        var result = await mediator.SendAsync(query, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = stringLocalizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Get garbage orders for a specific garbage group with optional filters.
    /// </summary>
    private static async Task<IResult> GetGarbageOrdersAsync(
        [FromRoute] Guid groupId,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromBody] GetGarbageOrdersRequest request,
        ICurrentUserService currentUserService,
        IMediator mediator,
        IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        var query = new GetGarbageGroupOrdersQuery(
            groupId,
            currentUserService.UserId,
            request.FromDate,
            request.ToDate,
            request.Statuses);

        var result = await mediator.SendAsync(query, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = stringLocalizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
}


/// <summary>
/// Request parameters for retrieving garbage orders for a group.
/// </summary>
public record GetGarbageOrdersRequest
{
    /// <summary>
    /// Start date filter (inclusive). If null no lower bound is applied.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// End date filter (inclusive). If null no upper bound is applied.
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// Pickup option filter. If null, no filter is applied.
    /// </summary>
    public PickupOption? PickupOption { get; init; }

    /// <summary>
    /// Array of order statuses to filter by. If empty, all statuses are returned.
    /// </summary>
    public GarbageOrderStatus[] Statuses { get; init; } = [];
}

/// <summary>
/// Garbage order request used to calculate estimated cost.
/// </summary>
public record GarbageOrderCalculationRequest
{
    /// <summary>
    /// Pickup option.
    /// </summary>
    public PickupOption PickupOption { get; init; }

    /// <summary>
    /// Container size if pickup option is container.
    /// </summary>
    public ContainerSize? ContainerSize { get; init; }

    /// <summary>
    /// Drop off date in case of container.
    /// </summary>
    public DateTime? DropOffDate { get; init; }

    /// <summary>
    /// Pickup date.
    /// </summary>
    public DateTime PickupDate { get; init; }

    /// <summary>
    /// Is high priority.
    /// </summary>
    public bool IsHighPriority { get; init; }

    /// <summary>
    /// Collecting service.
    /// </summary>
    public bool CollectingService { get; init; }
}

/// <summary>
/// Garbage order request containing step one and step two data.
/// </summary>
public record GarbageOrderRequest
{
    /// <summary>
    /// Pickup option.
    /// </summary>
    public PickupOption PickupOption { get; init; }
    
    /// <summary>
    /// Container size if pickup option is container.
    /// </summary>
    public ContainerSize? ContainerSize { get; init; }
    
    /// <summary>
    /// Drop off date in case of container.
    /// </summary>
    public DateTime? DropOffDate { get; init; }
    
    /// <summary>
    /// Pickup date.
    /// </summary>
    public DateTime PickupDate { get; init; }
    
    /// <summary>
    /// Is high priority.
    /// </summary>
    public bool IsHighPriority { get; init; }
    
    /// <summary>
    /// Collecting service.
    /// </summary>
    public bool CollectingService { get; init; }
    
    /// <summary>
    /// User IDs participating in the order.
    /// </summary>
    public ICollection<Guid> UserIds { get; init; } = [];
}

namespace WasteFree.Application.Features.GarbageOrders.Dtos;

public sealed record GarbageOrderDetailsDto(string? AssignedAdminAvatarUrl, IDictionary<Guid, string> UserAvatarsUrls);

namespace WasteFree.Application.Features.Account.Dtos;

public record UserStatsDto(
    decimal Savings,
    double WasteReduced,
    int Collections,
    int CommunityCount);

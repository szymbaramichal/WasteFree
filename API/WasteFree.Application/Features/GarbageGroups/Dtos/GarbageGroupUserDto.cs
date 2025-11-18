using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Interfaces;

namespace WasteFree.Application.Features.GarbageGroups.Dtos;

/// <summary>
/// DTO representing a user that is a member of a garbage group.
/// </summary>
public class GarbageGroupUserDto
{
    /// <summary>
    /// Identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The role of the user within the garbage group (for example Owner or Member).
    /// </summary>
    public GarbageGroupRole GarbageGroupRole { get; set; }

    /// <summary>
    /// Indicates whether the user's membership is pending (invited but not accepted).
    /// </summary>
    public bool IsPending { get; set; }

    /// <summary>
    /// Temporary read URL that points to the user's avatar image, if uploaded.
    /// </summary>
    public string? AvatarUrl { get; set; }
}

public static class GarbageGroupUserDtoExtensions
{
    private static readonly TimeSpan AvatarUrlTtl = TimeSpan.FromMinutes(5);

    public static ICollection<GarbageGroupUserDto> MapToGarbageGroupUserDto(
        this ICollection<UserGarbageGroup> users,
        IReadOnlyDictionary<Guid, string?>? avatarUrls = null)
    {
        var usersList = new List<GarbageGroupUserDto>(users.Count);

        foreach (var user in users)
        {
            string avatarUrl = string.Empty;

            avatarUrl = avatarUrls?.GetValueOrDefault(user.UserId) ?? "";

            usersList.Add(new GarbageGroupUserDto
            {
                Id = user.UserId,
                Username = user.User?.Username ?? string.Empty,
                GarbageGroupRole = user.Role,
                IsPending = user.IsPending,
                AvatarUrl = avatarUrl
            });
        }

        return usersList;
    }

    public static async Task<IReadOnlyDictionary<Guid, string?>> BuildAvatarUrlLookupAsync(
        this IEnumerable<UserGarbageGroup> users,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(blobStorageService);

        var avatarCandidates = users
            .Where(ug => !string.IsNullOrWhiteSpace(ug.User?.AvatarName))
            .GroupBy(ug => new { ug.UserId, V = ug.User!.AvatarName! })
            .ToList();

        if (avatarCandidates.Count == 0)
        {
            return new Dictionary<Guid, string?>();
        }

        var urlTasks = avatarCandidates.Select(async candidate =>
        {
            var avatarUrl = await blobStorageService.GetReadSasUrlAsync(
                BlobContainerNames.Avatars,
                candidate.Key.V,
                AvatarUrlTtl,
                cancellationToken);

            return new KeyValuePair<Guid, string?>(candidate.Key.UserId, avatarUrl);
        });

        var resolvedUrls = await Task.WhenAll(urlTasks);
        return resolvedUrls.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
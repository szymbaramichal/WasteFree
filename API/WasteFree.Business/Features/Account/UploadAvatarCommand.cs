using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Account.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Account;

public record UploadAvatarCommand(Guid UserId, IFormFile Avatar) : IRequest<ProfileDto>;

public class UploadAvatarCommandHandler(ApplicationDataContext context, IBlobStorageService blobStorage)
    : IRequestHandler<UploadAvatarCommand, ProfileDto>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    public async Task<Result<ProfileDto>> HandleAsync(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<ProfileDto>.Failure(ApiErrorCodes.NotFound, System.Net.HttpStatusCode.NotFound);
        }

        if (request.Avatar.Length == 0)
        {
            return Result<ProfileDto>.Failure(ApiErrorCodes.EmptyImage, HttpStatusCode.BadRequest);
        }
        
        if (request.Avatar.Length > MaxBytes)
        {
            return Result<ProfileDto>.Failure(ApiErrorCodes.TooBigImage, System.Net.HttpStatusCode.BadRequest);
        }
        
        var contentType = request.Avatar.ContentType;
        if (!AllowedContentTypes.Contains(contentType))
        {
            return Result<ProfileDto>.Failure(ApiErrorCodes.UnsupportedImageType, System.Net.HttpStatusCode.BadRequest);
        }

        var extension = Path.GetExtension(request.Avatar.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".bin"
            };
        }
        var blobName = $"{request.UserId}_avatar{extension}";

        await using var stream = request.Avatar.OpenReadStream();
        var sasUrl = await blobStorage.UploadAsync(stream, contentType, BlobContainerNames.Avatars, blobName, cancellationToken);
        user.AvatarName = blobName;
        
        await context.SaveChangesAsync(cancellationToken);
        return Result<ProfileDto>.Success(user.MapToProfileWithAvatarUrl(sasUrl));
    }
}
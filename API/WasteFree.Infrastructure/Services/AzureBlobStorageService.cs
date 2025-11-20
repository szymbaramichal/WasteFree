using System.Collections.Concurrent;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using WasteFree.Domain.Interfaces;

namespace WasteFree.Infrastructure.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly ConcurrentDictionary<string, CacheEntry> _avatarCache = new();
    private static readonly TimeSpan CacheSafetyMargin = TimeSpan.FromSeconds(5);

    public AzureBlobStorageService(string connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
            _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(Stream content, string contentType, string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Container name must be provided.", nameof(containerName));
        if (string.IsNullOrWhiteSpace(blobName)) throw new ArgumentException("Blob name must be provided.", nameof(blobName));

        var containerClient = _blobServiceClient!.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobName);
        var httpHeaders = new BlobHttpHeaders
        {
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        };

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

        var options = new BlobUploadOptions
        {
            HttpHeaders = httpHeaders
        };
        await blobClient.UploadAsync(content, options, cancellationToken);
        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5));

        InvalidateCache(containerName, blobName);

        return sasUri.ToString();
    }

    public async Task<string?> GetReadSasUrlAsync(string containerName, string blobName, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Container name must be provided.", nameof(containerName));
        if (string.IsNullOrWhiteSpace(blobName)) return string.Empty;

        var cacheKey = BuildCacheKey(containerName, blobName);
        if (_avatarCache.TryGetValue(cacheKey, out var entry) && entry.ExpiresAtUtc > DateTimeOffset.UtcNow)
        {
            return entry.Url;
        }

        var containerClient = _blobServiceClient!.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var exists = await blobClient.ExistsAsync(cancellationToken);
        if (!exists.Value)
        {
            return null;
        }

        if (!blobClient.CanGenerateSasUri)
        {
            return null;
        }

        var expiresAtUtc = DateTimeOffset.UtcNow.Add(ttl);
        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, expiresAtUtc);
        var sasUrl = sasUri.ToString();

        // subtract safety margin so cached entries expire a bit earlier than SAS token
        var cacheEntry = new CacheEntry(sasUrl, expiresAtUtc - CacheSafetyMargin);
        _avatarCache[cacheKey] = cacheEntry;

        return sasUrl;
    }

    private static string BuildCacheKey(string containerName, string blobName) => $"{containerName}::{blobName}";

    private void InvalidateCache(string containerName, string blobName)
    {
        var cacheKey = BuildCacheKey(containerName, blobName);
        _avatarCache.TryRemove(cacheKey, out _);
    }

    private sealed record CacheEntry(string Url, DateTimeOffset ExpiresAtUtc);
}

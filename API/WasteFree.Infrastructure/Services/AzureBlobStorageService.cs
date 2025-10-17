using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using WasteFree.Domain.Interfaces;

namespace WasteFree.Infrastructure.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Blob Storage connection string is missing.", nameof(connectionString));
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(Stream content, string contentType, string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Container name must be provided.", nameof(containerName));
        if (string.IsNullOrWhiteSpace(blobName)) throw new ArgumentException("Blob name must be provided.", nameof(blobName));

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
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

        return sasUri.ToString();
    }

    public async Task<string?> GetReadSasUrlAsync(string containerName, string blobName, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Container name must be provided.", nameof(containerName));
        if (string.IsNullOrWhiteSpace(blobName)) return string.Empty;

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
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

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(ttl));
        return sasUri.ToString();
    }
}

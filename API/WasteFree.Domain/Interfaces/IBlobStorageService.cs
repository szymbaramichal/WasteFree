namespace WasteFree.Shared.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string contentType, string containerName, string blobName, CancellationToken cancellationToken = default);

    Task<string?> GetReadSasUrlAsync(string containerName, string blobName, TimeSpan ttl, CancellationToken cancellationToken = default);
}

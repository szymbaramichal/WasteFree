namespace WasteFree.Shared.Interfaces;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads content to the specified container and blob name. Returns SAS URL.
    /// </summary>
    /// <param name="content">Stream positioned at the beginning; will not be disposed.</param>
    /// <param name="contentType">MIME content type to set on the blob.</param>
    /// <param name="containerName">Target container name.</param>
    /// <param name="blobName">Blob name (path) within the container.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SAS URL.</returns>
    Task<string> UploadAsync(Stream content, string contentType, string containerName, string blobName, CancellationToken cancellationToken = default);
}

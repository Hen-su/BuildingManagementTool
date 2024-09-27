using Azure.Storage.Blobs.Models;

namespace BuildingManagementTool.Models
{
    public interface IBlobService
    {
        Task<bool> BlobExistsAsync(string containerName, string blobName);
        Task<bool> UploadBlobAsync(string containerName, string blobName, Stream data, BlobHttpHeaders headers);
        Task<bool> DeleteBlobAsync(string containerName, string blobName);
        Task<string> GetBlobUrlAsync(string containerName, string blobName);
        Task<IEnumerable<string>> ListBlobsAsync(string containerName);
        Task<Stream> DownloadBlobAsync(string containerName, string blobName);
        Task<bool> DeleteByPrefix(string containerName, string prefix);
        Task RenameBlobDirectory(string containerName, string oldDirectory, string newDirectory);
        Task<Dictionary<int, List<string>>> GetBlobUrisByPrefix(string containerName, string prefix);
    }
}

using Azure.Storage.Blobs.Models;

namespace BuildingManagementTool.Models
{
    public interface IBlobService
    {
        Task<bool> BlobExistsAsync(string containerName, string blobName, string role);
        Task<bool> UploadBlobAsync(string containerName, string blobName, Stream data, BlobHttpHeaders headers, string role);
        Task<bool> DeleteBlobAsync(string containerName, string blobName, string role);
        Task<string> GetBlobUrlAsync(string containerName, string blobName, string role);
        Task<IEnumerable<string>> ListBlobsAsync(string containerName, string role);
        Task<Stream> DownloadBlobAsync(string containerName, string blobName, string role);
        Task<bool> DeleteByPrefix(string containerName, string prefix, string role);
        Task RenameBlobDirectory(string containerName, string oldDirectory, string newDirectory, string role);
        Task<Dictionary<int, List<string>>> GetBlobUrisByPrefix(string containerName, string prefix, string role);
        Task CreateBlobContainer(string containerName);
    }
}

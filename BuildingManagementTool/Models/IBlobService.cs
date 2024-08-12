namespace BuildingManagementTool.Models
{
    public interface IBlobService
    {
        Task<bool> BlobExistsAsync(string containerName, string blobName);
        Task<bool> UploadBlobAsync(string containerName, string blobName, Stream data);
        Task<bool> DeleteBlobAsync(string containerName, string blobName);
        Task<string> GetBlobUrlAsync(string containerName, string blobName);
        Task<IEnumerable<string>> ListBlobsAsync(string containerName);
    }
}

using Azure.Storage.Blobs;
using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<bool> BlobExistsAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync();
        }

        public async Task<bool> UploadBlobAsync(string containerName, string blobName, Stream data)
        {
            if (data == null) 
            {
                throw new ArgumentNullException(nameof(data));
            }
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(data);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> ListBlobsAsync(string containerName)
        {
            try { var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                List<string> blobNames = new List<string>();

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    blobNames.Add(blobItem.Name);
                }

                return blobNames;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                {
                    return null;
                }

                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0; 
                return memoryStream;
            }
            catch
            {
                return null;
            }
        }

    }
}

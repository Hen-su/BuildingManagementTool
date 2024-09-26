using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

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

        public async Task<bool> UploadBlobAsync(string containerName, string blobName, Stream data, BlobHttpHeaders headers)
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
                await blobClient.UploadAsync(data, headers);
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
                return await blobClient.DeleteIfExistsAsync();
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

        public async Task<string> GetBlobUrlAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            bool blobExists = await blobClient.ExistsAsync();
            if (!blobExists)
            {
                return null;
            }
            var blobUrl = blobClient.Uri.ToString();
            //blobUrl = blobUrl.StartsWith("http") ? blobUrl.Replace("http", "https") : blobUrl;
            return blobUrl;
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

        public async Task<bool> DeleteByPrefix(string containerName, string prefix)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                    await blobClient.DeleteIfExistsAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task RenameBlobDirectory(string containerName, string oldDirectory, string newDirectory)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: oldDirectory))
            {
                string oldBlobName = blobItem.Name;
                string newBlobName = oldBlobName.Replace(oldDirectory, newDirectory);
                BlobClient oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                BlobClient newBlobClient = containerClient.GetBlobClient(newBlobName);
                await newBlobClient.StartCopyFromUriAsync(oldBlobClient.Uri);

                BlobProperties properties = await newBlobClient.GetPropertiesAsync();
                if (properties.CopyStatus == CopyStatus.Success)
                {
                    await oldBlobClient.DeleteIfExistsAsync();
                }
            }
        }

        public async Task<Dictionary<int, List<string>>> GetBlobUrisByPrefix(string containerName, string prefix)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = containerClient.GetBlobsAsync(prefix: prefix);
            var blobList = new Dictionary<int, List<string>>();
            var i = 0;
            await foreach (var blob in blobs) 
            {
                var blobClient = containerClient.GetBlobClient(blob.Name);
                blobList.Add(i, new List<string> { blobClient.Name.Split('/').Last(), blobClient.Uri.ToString() });
                i++;
            }
            return blobList;
        }
    }
}

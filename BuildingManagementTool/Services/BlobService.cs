using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using System.Reflection.Metadata;

namespace BuildingManagementTool.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ISASTokenHandler _sasTokenHandler;
        private readonly IConfiguration _configuration;
        private readonly IBlobClientFactory _blobClientFactory;

        public BlobService(BlobServiceClient blobServiceClient, ISASTokenHandler sasTokenHandler, IConfiguration configuration, IBlobClientFactory blobClientFactory)
        {
            _blobServiceClient = blobServiceClient;
            _sasTokenHandler = sasTokenHandler;
            _configuration = configuration;
            _blobClientFactory = blobClientFactory;
        }

        public async Task CreateBlobContainer(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
        }

        public async Task<bool> BlobExistsAsync(string containerName, string blobName, string role)
        {
            string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);

            if (string.IsNullOrEmpty(sasToken))
            {
                throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
            }
            var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
            var blobClient = _blobClientFactory.CreateBlobClient(blobUri);
            return await blobClient.ExistsAsync();
        }

        public async Task<bool> UploadBlobAsync(string containerName, string blobName, Stream data, BlobHttpHeaders headers, string role)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            try
            {
                string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);

                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
                var blobClient = _blobClientFactory.CreateBlobClient(blobUri);
                await blobClient.UploadAsync(data, headers);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteBlobAsync(string containerName, string blobName, string role)
        {
            try
            {
                string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }
                var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
                var blobClient = _blobClientFactory.CreateBlobClient(blobUri);
                return await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> ListBlobsAsync(string containerName, string role)
        {
            try 
            {
                string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }

                var containerUri = new Uri($"https://{_configuration["AzureStorage:AccountName"]}.blob.core.windows.net/{containerName}{sasToken}");
                var containerClient = new BlobContainerClient(containerUri);

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

        public async Task<string> GetBlobUrlAsync(string containerName, string blobName, string role)
        {
            string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);
            if (string.IsNullOrEmpty(sasToken))
            {
                throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            bool blobExists = await blobClient.ExistsAsync();
            if (!blobExists)
            {
                return null;
            }
            var blobUrlWithSas = $"{blobClient.Uri}?{sasToken}";
            
            return blobUrlWithSas;
        }

        public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, string role)
        {
            try
            {
                string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }

                var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
                var blobClient = _blobClientFactory.CreateBlobClient(blobUri);

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

        public async Task<bool> DeleteByPrefix(string containerName, string prefix, string role)
        {
            try
            {
                string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobItem.Name}?{sasToken}");
                    BlobClient blobClient = _blobClientFactory.CreateBlobClient(blobUri);
                    await blobClient.DeleteIfExistsAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task RenameBlobDirectory(string containerName, string oldDirectory, string newDirectory, string role)
        {
            string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);
            if (string.IsNullOrEmpty(sasToken))
            {
                throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: oldDirectory))
            {
                string oldBlobName = blobItem.Name;
                string newBlobName = oldBlobName.Replace(oldDirectory, newDirectory);

                var oldBlobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{oldBlobName}");
                BlobClient oldBlobClient = _blobClientFactory.CreateBlobClientWithSAS(oldBlobUri, sasToken);

                var newBlobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{newBlobName}");
                BlobClient newBlobClient = _blobClientFactory.CreateBlobClientWithSAS(newBlobUri, sasToken);
                await newBlobClient.StartCopyFromUriAsync(oldBlobClient.Uri);

                BlobProperties properties = await newBlobClient.GetPropertiesAsync();
                if (properties.CopyStatus == CopyStatus.Success)
                {
                    await oldBlobClient.DeleteIfExistsAsync();
                }
            }
        }

        public async Task<Dictionary<int, List<string>>> GetBlobUrisByPrefix(string containerName, string prefix, string role)
        {
            string sasToken = await _sasTokenHandler.GetContainerSasTokenFromSession(containerName, role);
            if (string.IsNullOrEmpty(sasToken))
            {
                throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = containerClient.GetBlobsAsync(prefix: prefix);
            
            if (blobs != null)
            {
                var blobList = new Dictionary<int, List<string>>();
                var i = 0;
                await foreach (var blob in blobs) 
                {
                    var blobClient = containerClient.GetBlobClient(blob.Name);
                    var blobUriWithSas = $"{blobClient.Uri}?{sasToken}";
                    blobList.Add(i, new List<string> { blobClient.Name.Split('/').Last(), blobUriWithSas });
                    i++;
                }
                return blobList;
            }
            return null;
        }
    }
}

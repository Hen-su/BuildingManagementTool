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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public BlobService(BlobServiceClient blobServiceClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<string> GenerateAndStoreContainerSasToken(string containerName, string role)
        {
            // Get a reference to the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            if (!await containerClient.ExistsAsync())
            {
                throw new Exception($"Container '{containerName}' does not exist.");
            }

            // Create a SAS token for the container
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "c", // 'c' stands for container-level
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            // Assign permissions (e.g., Read, Write, List, Delete)
            if(role == "Manager")
            {
                sasBuilder.SetPermissions(BlobContainerSasPermissions.All);
            }
            else
            {
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List);
            }
            // Get the Storage Key to sign the SAS token)
            var accountName = _configuration["AzureStorage:AccountName"];
            var accountKey = _configuration["AzureStorage:AccountKey"];
            StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);

            // Generate the SAS token using the StorageSharedKeyCredential
            var sasToken = sasBuilder.ToSasQueryParameters(sharedKeyCredential);

            // Store the SAS token in session using the container name as the key
            _httpContextAccessor.HttpContext.Session.SetString($"containerSasToken_{containerName}", sasToken.ToString());

            return sasToken.ToString();
        }

        // Method to retrieve a container-level SAS token from the session
        public async Task<string> GetContainerSasTokenFromSession(string containerName, string role)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sasToken = session.GetString($"containerSasToken_{containerName}");
            if( sasToken == null )
            {
                sasToken = await GenerateAndStoreContainerSasToken(containerName, role);
            }
            var sasTokenParams = System.Web.HttpUtility.ParseQueryString(new Uri($"https://example.com?{sasToken}").Query);

            if (DateTimeOffset.TryParse(sasTokenParams["se"], out var expiryTime)) // "se" is the expiration time parameter
            {
                if (expiryTime < DateTimeOffset.UtcNow)
                {
                    // The token has expired
                    sasToken = await GenerateAndStoreContainerSasToken(containerName, role);
                    return sasToken;
                }
            }
            return sasToken;
        }

        public async Task CreateBlobContainer(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
        }

        public async Task<bool> BlobExistsAsync(string containerName, string blobName, string role)
        {
            string sasToken = await GetContainerSasTokenFromSession(containerName, role);

            if (string.IsNullOrEmpty(sasToken))
            {
                throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
            }
            var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
            var blobClient = new BlobClient(blobUri);
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
                string sasToken = await GetContainerSasTokenFromSession(containerName, role);

                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
                var blobClient = new BlobClient(blobUri);
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
                string sasToken = await GetContainerSasTokenFromSession(containerName, role);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }
                var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
                var blobClient = new BlobClient(blobUri);
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
                string sasToken = await GetContainerSasTokenFromSession(containerName, role);
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
            string sasToken = await GetContainerSasTokenFromSession(containerName, role);
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
                string sasToken = await GetContainerSasTokenFromSession(containerName, role);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }

                var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobName}?{sasToken}");
                var blobClient = new BlobClient(blobUri);

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
                string sasToken = await GetContainerSasTokenFromSession(containerName, role);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new InvalidOperationException($"SAS token for container '{containerName}' is missing or invalid.");
                }
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    var blobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{blobItem.Name}?{sasToken}");
                    BlobClient blobClient = new BlobClient(blobUri);
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
            string sasToken = await GetContainerSasTokenFromSession(containerName, role);
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
                BlobClient oldBlobClient = new BlobClient(oldBlobUri, new AzureSasCredential(sasToken));

                var newBlobUri = new Uri($"http://127.0.0.1:10000/{_configuration["AzureStorage:AccountName"]}/{containerName}/{newBlobName}");
                BlobClient newBlobClient = new BlobClient(newBlobUri, new AzureSasCredential(sasToken));
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
            string sasToken = await GetContainerSasTokenFromSession(containerName, role);
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

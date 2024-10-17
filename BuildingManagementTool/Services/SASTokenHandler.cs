using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using BuildingManagementTool.Models;

namespace BuildingManagementTool.Services
{
    public class SASTokenHandler : ISASTokenHandler
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SASTokenHandler(BlobServiceClient blobServiceClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // Method to retrieve a container-level SAS token from the session
        public async Task<string> GetContainerSasTokenFromSession(string containerName, string role)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sasToken = session.GetString($"containerSasToken_{containerName}");
            if (sasToken == null)
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
        
        private async Task<string> GenerateAndStoreContainerSasToken(string containerName, string role)
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
            if (role == "Manager")
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
    }
}

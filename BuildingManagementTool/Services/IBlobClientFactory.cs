using Azure;
using Azure.Storage.Blobs;

namespace BuildingManagementTool.Services
{
    public interface IBlobClientFactory
    {
        BlobClient CreateBlobClient(Uri blobUri);
        BlobClient CreateBlobClientWithSAS(Uri blobUri, string sasToken);
    }

    public class BlobClientFactory : IBlobClientFactory
    {
        public BlobClient CreateBlobClient(Uri blobUri)
        {
            return new BlobClient(blobUri);
        }

        public BlobClient CreateBlobClientWithSAS(Uri blobUri, string sasToken) 
        {
            return new BlobClient(blobUri, new AzureSasCredential(sasToken));
        }
    }
}

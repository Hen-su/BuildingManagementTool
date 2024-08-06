using Azure.Storage.Blobs;

namespace BuildingManagementTool.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }
    }
}

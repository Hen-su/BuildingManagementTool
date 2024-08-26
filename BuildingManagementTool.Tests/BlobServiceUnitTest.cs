using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class BlobServiceUnitTest
    {
        private Mock<BlobServiceClient> _mockBlobServiceClient;
        private Mock<BlobContainerClient> _mockBlobContainerClient;
        private Mock<BlobClient> _mockBlobClient;
        private BlobService _blobService;

        [SetUp]
        public void Setup()
        {
            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockBlobContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();

            _mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _blobService = new BlobService(_mockBlobServiceClient.Object);
        }

        [Test]
        public async Task UploadBlobAsync_FileExists_Success()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            var data = new byte[] { 1, 2, 3, 4, 5 };
            var content = new MemoryStream(data);
            BlobHttpHeaders headers = new BlobHttpHeaders
            {
                ContentType = "text/plain"
            };

            var mockBlobContentInfo = BlobsModelFactory.BlobContentInfo(
                eTag: new ETag(""),
                lastModified: DateTimeOffset.MinValue,
                encryptionKeySha256: "dummyKey",
                encryptionScope: "default",
                blobSequenceNumber: 1,
                versionId: "version1",
                contentHash: Convert.FromBase64String("dGVzdGhhc2g=")
               ); 

            var mockResponse = Response.FromValue(mockBlobContentInfo, null);

            _mockBlobClient.Setup(x => x.UploadAsync(
                It.IsAny<Stream>(),                           
                It.Is<BlobHttpHeaders>(h => h.ContentType == "text/plain"), 
                It.IsAny<IDictionary<string, string>>(),    
                It.IsAny<BlobRequestConditions>(),           
                It.IsAny<IProgress<long>>(),              
                It.IsAny<AccessTier?>(),                    
                It.IsAny<StorageTransferOptions>(),       
                It.IsAny<CancellationToken>()))            
                .ReturnsAsync(mockResponse);

            var response = await _blobService.UploadBlobAsync(containerName, blobName, content, headers);

            _mockBlobClient.Verify(x => x.UploadAsync(
                It.IsAny<Stream>(),
                It.Is<BlobHttpHeaders>(h => h.ContentType == "text/plain"),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<IProgress<long>>(),
                It.IsAny<AccessTier?>(),
                It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>()), 
                Times.Once());

            Assert.True(response);
        }

        [Test]
        public async Task UploadBlobAsync_FileNull_Fail()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            Stream content = null;
            BlobHttpHeaders headers = null;

            var mockBlobContentInfo = BlobsModelFactory.BlobContentInfo(
                eTag: new ETag(""),
                lastModified: DateTimeOffset.MinValue,
                encryptionKeySha256: "dummyKey",
                encryptionScope: "default",
                blobSequenceNumber: 1,
                versionId: "version1",
                contentHash: Convert.FromBase64String("dGVzdGhhc2g=")
               );
            var mockResponse = Response.FromValue(mockBlobContentInfo, null);

            _mockBlobClient.Setup(x => x.UploadAsync(
                It.IsAny<Stream>(),
                It.Is<BlobHttpHeaders>(h => h.ContentType == "text/plain"),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<IProgress<long>>(),
                It.IsAny<AccessTier?>(),
                It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _blobService.UploadBlobAsync(containerName, blobName, content, headers));

            _mockBlobClient.Verify(x => x.UploadAsync(
                It.IsAny<Stream>(),
                It.Is<BlobHttpHeaders>(h => h.ContentType == "text/plain"),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<IProgress<long>>(),
                It.IsAny<AccessTier?>(),
                It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);

            Assert.That(ex.Message, Does.Contain("Value cannot be null"));
        }

        [Test]
        public async Task DeleteBlobAsync_FileExists_Success()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            mockResponse.Setup(m => m.Value).Returns(true);

            _mockBlobClient.Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);

            var response = await _blobService.DeleteBlobAsync(containerName, blobName);

            _mockBlobClient.Verify(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(response);
        }

        [Test]
        public async Task DeleteBlobAsync_FileNotExists_Fail()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            mockResponse.Setup(m => m.Value).Returns(false);

            _mockBlobClient.Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);

            var response = await _blobService.DeleteBlobAsync(containerName, blobName);

            _mockBlobClient.Verify(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.False(response);
        }

        [Test]
        public async Task GetBlobUrlAsync_BlobExists_Success()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            var expectedUrl = "https://example.com/test/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            mockResponse.Setup(m => m.Value).Returns(true);

            _mockBlobClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);
            _mockBlobClient.SetupGet(x => x.Uri)
            .Returns(new Uri(expectedUrl));

            var result = await _blobService.GetBlobUrlAsync(containerName, blobName);
            Assert.AreEqual(result, expectedUrl);
        }

        [Test]
        public async Task GetBlobUrlAsync_BlobNotExist_Success()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            var expectedUrl = "https://example.com/test/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            mockResponse.Setup(m => m.Value).Returns(false);

            _mockBlobClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);
            _mockBlobClient.SetupGet(x => x.Uri)
            .Returns(new Uri(expectedUrl));

            var result = await _blobService.GetBlobUrlAsync(containerName, blobName);
            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task DownloadBlobAsync_BlobExists_ReturnsStream()
        {
            var containerName = "test-container";
            var blobName = "test-blob";
            var mockStream = new MemoryStream();
            var mockBlobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: mockStream);

            _mockBlobClient
                .Setup(blob => blob.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, null));

            _mockBlobClient
                .Setup(blob => blob.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(mockBlobDownloadInfo, null));

            var result = await _blobService.DownloadBlobAsync(containerName, blobName);

            Assert.IsNotNull(result);
            Assert.AreEqual(mockStream, result);
        }

        [Test]
        public async Task DownloadBlobAsync_BlobDoesNotExist_Fail_ReturnsNull()
        {
            var containerName = "test";
            var blobName = "category/test.txt";

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(blobName))
                .Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(client => client.ExistsAsync(default))
                .ReturnsAsync(Response.FromValue(false, null));

            var result = await _blobService.DownloadBlobAsync(containerName, blobName);

            Assert.IsNull(result);
        }

        [Test]
        public async Task DownloadBlobAsync_ExceptionThrown_ReturnsNull()
        {
            var containerName = "test";
            var blobName = "category/test.txt";

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                .Throws(new Exception("Test exception"));

            var result = await _blobService.DownloadBlobAsync(containerName, blobName);

            Assert.IsNull(result);
        }
    }
}

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
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
        public async Task UploadBlobAsync_File_Success()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            var data = new byte[] { 1, 2, 3, 4, 5 };
            var content = new MemoryStream(data);

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

            _mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<Stream>()))
           .ReturnsAsync(mockResponse);

            var response = await _blobService.UploadBlobAsync(containerName, blobName, content);

            _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>()), Times.Once);
            Assert.True(response);
        }

        [Test]
        public async Task UploadBlobAsync_FileNull_Fail()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            Stream content = null;

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

            _mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<Stream>()))
           .ReturnsAsync(mockResponse);

            var response = await _blobService.UploadBlobAsync(containerName, blobName, content);

            _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>()), Times.Never);
            Assert.False(response);
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
            Assert.True(response);
        }
    }
}

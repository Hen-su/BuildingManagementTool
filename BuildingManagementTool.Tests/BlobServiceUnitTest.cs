using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Castle.Components.DictionaryAdapter.Xml;
using Castle.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;

namespace BuildingManagementTool.Tests
{
    internal class BlobServiceUnitTest
    {
        private Mock<BlobServiceClient> _mockBlobServiceClient;
        private Mock<BlobContainerClient> _mockBlobContainerClient;
        private Mock<BlobClient> _mockBlobClient;
        private Mock<ISASTokenHandler> _mockSasTokenHandler;
        private Mock<Microsoft.Extensions.Configuration.IConfiguration> _mockConfiguration;
        private Mock<IBlobClientFactory> _mockBlobClientFactory;
        private BlobService _blobService;

        [SetUp]
        public void Setup()
        {
            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockBlobContainerClient = new Mock<BlobContainerClient>();
            _mockSasTokenHandler = new Mock<ISASTokenHandler>();
            _mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _mockBlobClientFactory = new Mock<IBlobClientFactory>();

            _mockBlobClient = new Mock<BlobClient>();
            _mockConfiguration.Setup(_ => _["AzureStorage:AccountName"]).Returns("devstoreaccount1");
            _mockBlobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);
            _mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _blobService = new BlobService(_mockBlobServiceClient.Object, _mockSasTokenHandler.Object, _mockConfiguration.Object, _mockBlobClientFactory.Object);
        }

        [Test]
        public async Task UploadBlobAsync_FileExists_Success()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            var data = new byte[] { 1, 2, 3, 4, 5 };
            var content = new MemoryStream(data);
            var role = "Manager";
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

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobClientFactory.Setup(b => b.CreateBlobClient(It.IsAny<Uri>())).Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(x => x.UploadAsync(
                It.IsAny<Stream>(),
                It.Is<BlobHttpHeaders>(h => h.ContentType == "text/plain"),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<IProgress<long>>(),
                It.IsAny<AccessTier?>(),
                It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);


            var result = await _blobService.UploadBlobAsync(containerName, blobName, content, headers, role);

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

            Assert.True(result);
        }

        [Test]
        public async Task UploadBlobAsync_FileNull_Fail()
        {
            var containerName = "test";
            var blobName = "category/test.txt";
            Stream content = null;
            BlobHttpHeaders headers = null;
            var role = "Manager";

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
                await _blobService.UploadBlobAsync(containerName, blobName, content, headers, role));

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
            var blobName = "property/category/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            var role = "Manager";
            mockResponse.Setup(m => m.Value).Returns(true);

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobClientFactory.Setup(b => b.CreateBlobClient(It.IsAny<Uri>())).Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);

            var response = await _blobService.DeleteBlobAsync(containerName, blobName, role);

            _mockBlobClient.Verify(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(response);
        }

        [Test]
        public async Task DeleteBlobAsync_FileNotExists_Fail()
        {
            var containerName = "test";
            var blobName = "property/category/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            var role = "Manager";
            mockResponse.Setup(m => m.Value).Returns(false);

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobClientFactory.Setup(b => b.CreateBlobClient(It.IsAny<Uri>())).Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);

            var response = await _blobService.DeleteBlobAsync(containerName, blobName, role);

            _mockBlobClient.Verify(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.False(response);
        }

        [Test]
        public async Task GetBlobUrlAsync_BlobExists_Success()
        {
            var containerName = "test";
            var blobName = "property/category/test.txt";
            var blobUrl = "https://example.com/test/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            var role = "Manager";
            mockResponse.Setup(m => m.Value).Returns(true);

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);
            _mockBlobClient.SetupGet(x => x.Uri)
            .Returns(new Uri(blobUrl));

            var expectedUrl = new Uri($"{ blobUrl }?{validSasToken}");

            var result = await _blobService.GetBlobUrlAsync(containerName, blobName, role);
            Assert.AreEqual(result, expectedUrl.ToString());
        }

        [Test]
        public async Task GetBlobUrlAsync_BlobNotExist_Success()
        {
            var containerName = "test";
            var blobName = "property/category/test.txt";
            var expectedUrl = "https://example.com/test/test.txt";
            var mockResponse = new Mock<Response<bool>>();
            var role = "Manager";
            mockResponse.Setup(m => m.Value).Returns(false);

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockResponse.Object);
            _mockBlobClient.SetupGet(x => x.Uri)
            .Returns(new Uri(expectedUrl));

            var result = await _blobService.GetBlobUrlAsync(containerName, blobName, role);
            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task DownloadBlobAsync_BlobExists_ReturnsStream()
        {
            var containerName = "test-container";
            var blobName = "test-blob";
            var mockStream = new MemoryStream();
            var mockBlobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: mockStream);
            var role = "Manager";

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobClientFactory.Setup(b => b.CreateBlobClient(It.IsAny<Uri>())).Returns(_mockBlobClient.Object);

            _mockBlobClient
                .Setup(blob => blob.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, null));

            _mockBlobClient
                .Setup(blob => blob.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(mockBlobDownloadInfo, null));

            var result = await _blobService.DownloadBlobAsync(containerName, blobName, role);

            Assert.IsNotNull(result);
            Assert.AreEqual(mockStream, result);
        }

        [Test]
        public async Task DownloadBlobAsync_BlobDoesNotExist_Fail_ReturnsNull()
        {
            var containerName = "test";
            var blobName = "property/category/test.txt";
            var role = "Manager";

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(blobName))
                .Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(client => client.ExistsAsync(default))
                .ReturnsAsync(Response.FromValue(false, null));

            var result = await _blobService.DownloadBlobAsync(containerName, blobName, role);

            Assert.IsNull(result);
        }

        [Test]
        public async Task DownloadBlobAsync_ExceptionThrown_ReturnsNull()
        {
            var containerName = "test";
            var blobName = "property/category/test.txt";
            var role = "Manager";

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                .Throws(new Exception("Test exception"));

            var result = await _blobService.DownloadBlobAsync(containerName, blobName, role);

            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteByPrefix_DeleteSuccess_ReturnTrue()
        {
            var containerName = "test";
            var prefix = "property/category";
            var blobName1 = "property/category/test.txt";
            var blobName2 = "property/category/test2.txt";
            var blobList = new BlobItem[]
            {
                BlobsModelFactory.BlobItem(blobName1),
                BlobsModelFactory.BlobItem(blobName2),
            };
            var role = "Manager";

            Page<BlobItem> page = Page<BlobItem>.FromValues(blobList, null, Mock.Of<Response>());
            AsyncPageable<BlobItem> pageableBlobList = AsyncPageable<BlobItem>.FromPages(new[] { page });

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _mockBlobContainerClient
                .Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(pageableBlobList);

            _mockBlobClientFactory.Setup(b => b.CreateBlobClient(It.IsAny<Uri>())).Returns(_mockBlobClient.Object);

            _mockBlobContainerClient.Setup(x => x.DeleteIfExistsAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(true, null));

            var result = await _blobService.DeleteByPrefix(containerName, prefix, role);

            _mockBlobContainerClient
                .Verify(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            _mockBlobClient
                .Verify(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteByPrefix_DeleteFailed_ReturnFalse()
        {
            var containerName = "test";
            var prefix = "property/category";
            var blobName1 = "property/category/test.txt";
            var blobName2 = "property/category/test2.txt";
            var blobList = new BlobItem[]
            {
                BlobsModelFactory.BlobItem(blobName1),
                BlobsModelFactory.BlobItem(blobName2),
            };
            var role = "Manager";

            Page<BlobItem> page = Page<BlobItem>.FromValues(blobList, null, Mock.Of<Response>());
            AsyncPageable<BlobItem> pageableBlobList = AsyncPageable<BlobItem>.FromPages(new[] { page });

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _mockBlobClientFactory.Setup(b => b.CreateBlobClient(It.IsAny<Uri>())).Returns(_mockBlobClient.Object);

            _mockBlobContainerClient
                .Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Simulated Exception"));

            var result = await _blobService.DeleteByPrefix(containerName, prefix, role);

            _mockBlobContainerClient
                .Verify(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            _mockBlobClient
                .Verify(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task RenameBlobDirectory_ReturnSuccess()
        {
            var containerName = "test";
            var oldDirectory = "test/old-directory";
            var newDirectory = "test/new-directory";
            var oldBlobName1 = "test/old-directory/blob.txt";
            var newBlobName1 = "test/new-directory/blob.txt";

            var _mockOldBlobClient = new Mock<BlobClient>();
            var _mockNewBlobClient = new Mock<BlobClient>();

            var oldBlobUri = new Uri($"http://127.0.0.1:10000/devstoreaccount1/{containerName}/{oldBlobName1}");
            var newBlobUri = new Uri($"http://127.0.0.1:10000/devstoreaccount1/{containerName}/{newBlobName1}");

            var role = "Manager";

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockOldBlobClient.SetupGet(client => client.Uri).Returns(oldBlobUri);
            _mockNewBlobClient.SetupGet(client => client.Uri).Returns(newBlobUri);

            var blobList = new BlobItem[]
            {
                BlobsModelFactory.BlobItem(oldBlobName1)
            };

            Page<BlobItem> page = Page<BlobItem>.FromValues(blobList, null, Mock.Of<Response>());
            AsyncPageable<BlobItem> pageableBlobList = AsyncPageable<BlobItem>.FromPages(new[] { page });

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(oldBlobName1)).Returns(_mockOldBlobClient.Object);
            _mockBlobContainerClient.Setup(container => container.GetBlobClient(newBlobName1)).Returns(_mockNewBlobClient.Object);

            _mockBlobContainerClient.Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(),
            It.IsAny<BlobStates>(),
            It.Is<string>(prefix => prefix == oldDirectory), 
            It.IsAny<CancellationToken>())).Returns(pageableBlobList);

            var properties = BlobsModelFactory.BlobProperties(
                    lastModified: default,
                    createdOn: default,
                    metadata: null,
                    objectReplicationDestinationPolicyId: null,
                    objectReplicationSourceProperties: null,
                    blobType: BlobType.Block,
                    copyCompletedOn: default,
                    copyStatusDescription: null,
                    copyId: null,
                    copyProgress: null,
                    copySource: default,
                    blobCopyStatus: CopyStatus.Success,
                    isIncrementalCopy: false,
                    destinationSnapshot: null,
                    leaseDuration: LeaseDurationType.Infinite,
                    leaseState: LeaseState.Available,
                    leaseStatus: LeaseStatus.Locked,
                    contentLength: 1024,
                    contentType: null,
                    eTag: new ETag("test-etag"),
                    contentHash: null,
                    contentEncoding: null,
                    contentDisposition: null,
                    contentLanguage: null,
                    cacheControl: null,
                    blobSequenceNumber: 0,
                    acceptRanges: null,
                    blobCommittedBlockCount: 0,
                    isServerEncrypted: false,
                    encryptionKeySha256: null,
                    encryptionScope: null,
                    accessTier: "Hot",
                    accessTierInferred: false,
                    archiveStatus: null,
                    accessTierChangedOn: default,
                    versionId: null,
                    isLatestVersion: false,
                    tagCount: 0,
                    expiresOn: default,
                    isSealed: false,
                    rehydratePriority: null,
                    lastAccessed: default,
                    immutabilityPolicy: null,
                    hasLegalHold: false
                );

            _mockBlobClientFactory
                .Setup(factory => factory.CreateBlobClientWithSAS(oldBlobUri, It.IsAny<string>()))
                .Returns(_mockOldBlobClient.Object);

            _mockBlobClientFactory
                .Setup(factory => factory.CreateBlobClientWithSAS(newBlobUri, It.IsAny<string>()))
                .Returns(_mockNewBlobClient.Object);

            _mockNewBlobClient.Setup(client => client.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(properties, Mock.Of<Response>()));

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            _mockNewBlobClient.Setup(client => client.StartCopyFromUriAsync(
                It.IsAny<Uri>(),
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(mockCopyOperation.Object);

            _mockOldBlobClient.Setup(client => client.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            await _blobService.RenameBlobDirectory(containerName, oldDirectory, newDirectory, role);
            _mockNewBlobClient.Verify(client => client.StartCopyFromUriAsync(It.IsAny<Uri>(),
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()), Times.Once);

            _mockOldBlobClient.Verify(client => client.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RenameBlobDirectory_NullValue_NoInvocation()
        {
            var containerName = "test";
            string oldDirectory = null;
            var newDirectory = "test/new-directory";
            var oldBlobName1 = "test/old-directory/blob.txt";
            var newBlobName1 = "test/new-directory/blob.txt";

            var _mockOldBlobClient = new Mock<BlobClient>();
            var _mockNewBlobClient = new Mock<BlobClient>();

            var oldBlobUri = new Uri($"https://example.com/{oldBlobName1}");
            var newBlobUri = new Uri($"https://example.com/{newBlobName1}");

            var role = "Manager";

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockOldBlobClient.SetupGet(client => client.Uri).Returns(oldBlobUri);
            _mockNewBlobClient.SetupGet(client => client.Uri).Returns(newBlobUri);

            var blobList = new BlobItem[]
            {
                BlobsModelFactory.BlobItem(oldBlobName1)
            };

            Page<BlobItem> page = Page<BlobItem>.FromValues(blobList, null, Mock.Of<Response>());
            AsyncPageable<BlobItem> pageableBlobList = AsyncPageable<BlobItem>.FromPages(new[] { page });

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(oldBlobName1)).Returns(_mockOldBlobClient.Object);
            _mockBlobContainerClient.Setup(container => container.GetBlobClient(newBlobName1)).Returns(_mockNewBlobClient.Object);

            _mockBlobContainerClient.Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(),
        It.IsAny<BlobStates>(),
        It.Is<string>(prefix => prefix == oldDirectory),
        It.IsAny<CancellationToken>())).Returns(pageableBlobList);

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            _mockNewBlobClient.Setup(client => client.StartCopyFromUriAsync(
                It.IsAny<Uri>(),
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(mockCopyOperation.Object);

            _mockOldBlobClient.Setup(client => client.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                () => _blobService.RenameBlobDirectory(containerName, oldDirectory, newDirectory, role));

            Assert.That(exception.Message, Does.Contain("Value cannot be null. (Parameter 'oldValue')"));
            _mockNewBlobClient.Verify(client => client.StartCopyFromUriAsync(It.IsAny<Uri>(),
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()), Times.Never);

            _mockOldBlobClient.Verify(client => client.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetBlobUrisByPrefix_Valid_ReturnDictionary()
        {
            var containerName = "user";
            var prefix = "property/images/";
            var blobName1 = "property/images/test.jpg";
            var blobName2 = "property/images/test2.jpg";
            var blob1Uri = new Uri("https://testaccount.blob.core.windows.net/property/images/test.jpg");
            var blob2Uri = new Uri("https://testaccount.blob.core.windows.net/property/images/test2.jpg");
            var blobList = new BlobItem[]
            {
                BlobsModelFactory.BlobItem(blobName1),
                BlobsModelFactory.BlobItem(blobName2),
            };
            var role = "Manager";

            Page<BlobItem> page = Page<BlobItem>.FromValues(blobList, null, Mock.Of<Response>());
            AsyncPageable<BlobItem> pageableBlobList = AsyncPageable<BlobItem>.FromPages(new[] { page });

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);
            
            _mockBlobContainerClient
                .Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(pageableBlobList);

            var mockBlobClient1 = new Mock<BlobClient>();
            mockBlobClient1.SetupGet(client => client.Uri)
                .Returns(blob1Uri);
            mockBlobClient1.SetupGet(client => client.Name)
                .Returns(blobName1);

            var mockBlobClient2 = new Mock<BlobClient>();
            mockBlobClient2.SetupGet(client => client.Uri)
                .Returns(blob2Uri);
            mockBlobClient2.SetupGet(client => client.Name)
                .Returns(blobName2);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(blobName1))
                .Returns(mockBlobClient1.Object);

            _mockBlobContainerClient.Setup(container => container.GetBlobClient(blobName2))
                .Returns(mockBlobClient2.Object);

            var returnedList = await _blobService.GetBlobUrisByPrefix(containerName, prefix, role);
            Assert.That(returnedList.Count(), Is.EqualTo(2));
            Assert.That(returnedList[0][1], Is.EqualTo( $"{ blob1Uri.ToString()}?{ validSasToken}" ));
            Assert.That(returnedList[1][1], Is.EqualTo($"{blob2Uri.ToString()}?{validSasToken}"));
        }

        [Test]
        public async Task GetBlobUrisByPrefix_NoBlobs_ReturnNull()
        {
            var containerName = "user";
            var prefix = "property/images/";
            var role = "Manager";

            var emptyBlobList = new BlobItem[0];
            Page<BlobItem> emptyPage = Page<BlobItem>.FromValues(emptyBlobList, null, Mock.Of<Response>());
            AsyncPageable<BlobItem> emptyPageableBlobList = AsyncPageable<BlobItem>.FromPages(new[] { emptyPage });

            var validSasToken = "sv=2018-03-28&st=2024-10-15T15%3A51%3A48Z&se=2024-10-16T15%3A51%3A48Z&sr=c&sp=rl&sig=nCXd7ENudz1Ohr8muwLFNp1a8WVxHAhIaydCfcjGeZ8%3D";
            _mockSasTokenHandler.Setup(s => s.GetContainerSasTokenFromSession(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validSasToken);

            _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient
                .Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(emptyPageableBlobList);

            var returnedList = await _blobService.GetBlobUrisByPrefix(containerName, prefix, role);
            Assert.IsEmpty(returnedList);
        }
    }
}
using Azure.Storage.Blobs;
using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class BlobControllerUnitTest
    {
        private Mock<IBlobService> _mockBlobService;
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private BlobController _blobController;
        [SetUp]
        public void Setup()
        {
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockBlobService = new Mock<IBlobService>();
            _blobController = new BlobController(_mockBlobService.Object, _mockDocumentRepository.Object);
        }

        [Test]
        public async Task UploadBlob_ValidInputSingle_Success()
        {
            var fileName = "test.txt";
            var content = "Hello World";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(1024);
            formFile.Setup(f => f.FileName).Returns("testfile.txt");
            formFile.Setup(f => f.ContentType).Returns("text/plain");
            formFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file content.")));
            formFile.Setup(f => f.Headers).Returns(new HeaderDictionary());

            var mockFiles = new List<IFormFile> { formFile.Object };

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(true);

            var result = await _blobController.UploadBlob(mockFiles) as RedirectToActionResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Document", result.ControllerName);
        }

        [Test]
        public async Task UploadBlob_ValidInputMultiple_Success()
        {
            var fileName = "test.txt";
            var content = "Hello World";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(1024);
            formFile.Setup(f => f.FileName).Returns("testfile.txt");
            formFile.Setup(f => f.ContentType).Returns("text/plain");
            formFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file content.")));
            formFile.Setup(f => f.Headers).Returns(new HeaderDictionary());

            var mockFiles = new List<IFormFile> { formFile.Object, formFile.Object };

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(true);

            var result = await _blobController.UploadBlob(mockFiles) as RedirectToActionResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Exactly(2));
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Document", result.ControllerName);
        }

        [Test]
        public async Task UploadBlob_NoInput_Fail()
        {
            var mockFiles = new List<IFormFile>();

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(false);

            var result = await _blobController.UploadBlob(mockFiles);

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Exactly(0));
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [TearDown]
        public void Teardown()
        {
            _blobController.Dispose();
        }
    }
}

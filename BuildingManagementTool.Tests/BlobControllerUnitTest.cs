using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(true);

            var result = await _blobController.UploadBlob(mockFiles) as RedirectToActionResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Once);
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

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(true);

            var result = await _blobController.UploadBlob(mockFiles) as RedirectToActionResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Exactly(2));
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Document", result.ControllerName);
        }

        [Test]
        public async Task UploadBlob_NoInput_Fail()
        {
            var mockFiles = new List<IFormFile>();

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(false);

            var result = await _blobController.UploadBlob(mockFiles);

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Exactly(0));
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteBlob_FileExists_Success()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.txt"
            };
        
            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _blobController.DeleteBlob(document.DocumentId) as RedirectToActionResult;

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Document", result.ControllerName);
        }

        [Test]
        public async Task DeleteBlob_FileNotExists_Fail()
        {
            var documentId = 1;
            var document = new Document();

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _blobController.DeleteBlob(document.DocumentId);

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.AreEqual("The File MetaData was not found", problemDetails.Detail);
            Assert.AreEqual("Metadata Not Found", problemDetails.Title);
            Assert.AreEqual(StatusCodes.Status404NotFound, problemDetails.Status);
        }

        [Test]
        public async Task DeleteBlob_BlobDeleteFailed_Fail()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.txt"
            };

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _blobController.DeleteBlob(document.DocumentId);

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.AreEqual("An error occurred when deleting the file", problemDetails.Detail);
            Assert.AreEqual("Deletion Error", problemDetails.Title);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, problemDetails.Status);
        }

        [Test]
        public async Task RenderFile_PDFExists_Success()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.pdf",
                ContentType = "application/pdf"
            };

            var blobUrl = "https://devstoreaccount1/test/category/test.pdf";

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(blobUrl);

            var result = await _blobController.RenderFile(documentId) as PartialViewResult;
            _mockBlobService.Verify(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("_PDFViewer", result.ViewName);
            Assert.AreEqual(blobUrl, result.Model);
        }

        [Test]
        public async Task RenderFile_VideoExists_Success()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.pdf",
                ContentType = "video/mp4"
            };

            var blobUrl = "https://devstoreaccount1/test/category/test.mp4";

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(blobUrl);

            var result = await _blobController.RenderFile(documentId) as PartialViewResult;

            _mockBlobService.Verify(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("_VideoPlayer", result.ViewName);
            Assert.AreEqual(blobUrl, result.Model);
        }

        [Test]
        public async Task RenderFile_FileDataNotFound_Fail()
        {
            var documentId = 1;
            Document document = null;

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string) null);

            var result = await _blobController.RenderFile(documentId);
            _mockBlobService.Verify(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.AreEqual("The File MetaData was not found", problemDetails.Detail);
            Assert.AreEqual("Metadata Not Found", problemDetails.Title);
            Assert.AreEqual(StatusCodes.Status404NotFound, problemDetails.Status);
        }

        [Test]
        public async Task RenderFile_FileNotExists_Fail()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.pdf",
                ContentType = "video/mp4"
            };

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string)null);

            var result = await _blobController.RenderFile(documentId);
            _mockBlobService.Verify(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.AreEqual("The file was not found in blob storage", problemDetails.Detail);
            Assert.AreEqual("File Not Found", problemDetails.Title);
            Assert.AreEqual(StatusCodes.Status404NotFound, problemDetails.Status);
        }

        [TearDown]
        public void Teardown()
        {
            _blobController.Dispose();
        }
    }
}

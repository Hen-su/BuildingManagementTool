using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
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
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private BlobController _blobController;

        [SetUp]
        public void Setup()
        {
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockBlobService = new Mock<IBlobService>();
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _blobController = new BlobController(_mockBlobService.Object, _mockDocumentRepository.Object, _mockPropertyCategoryRepository.Object);
        }

        [Test]
        public async Task UploadBlob_ValidInputSingle_Success()
        {
            var propertyCategory = 1;
            Category category = new Category { CategoryId = 1, CategoryName = "Plumbing" };
            var content = "Hello World";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(1024);
            formFile.Setup(f => f.FileName).Returns("testfile.txt");
            formFile.Setup(f => f.ContentType).Returns("text/plain");
            formFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file content.")));
            formFile.Setup(f => f.Headers).Returns(new HeaderDictionary());

            var mockFiles = new List<IFormFile> { formFile.Object };

            _mockPropertyCategoryRepository.Setup(pc => pc.GetById(It.IsAny<int>())).ReturnsAsync(new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1, Category = category });
            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(true);

            var result = await _blobController.UploadBlob(mockFiles, propertyCategory) as JsonResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Once);
            Assert.That(result.Value.ToString().Equals("{ success = True }"));
        }
    
    
        [Test]
        public async Task UploadBlob_ValidInputMultiple_Success()
        {
            var propertyCategory = 1;
            Category category = new Category { CategoryId = 1, CategoryName = "Plumbing" };
            var content = "Hello World";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(1024);
            formFile.Setup(f => f.FileName).Returns("testfile.txt");
            formFile.Setup(f => f.ContentType).Returns("text/plain");
            formFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file content.")));
            formFile.Setup(f => f.Headers).Returns(new HeaderDictionary());

            var mockFiles = new List<IFormFile> { formFile.Object, formFile.Object };

            _mockPropertyCategoryRepository.Setup(pc => pc.GetById(It.IsAny<int>())).ReturnsAsync(new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1, Category = category });
            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(true);

            var result = await _blobController.UploadBlob(mockFiles, propertyCategory) as JsonResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Exactly(2));
            Assert.That(result.Value.ToString().Equals("{ success = True }"));
        }
        
        [Test]
        public async Task UploadBlob_NoInput_Fail()
        {
            var propertyCategory = 1;
            Category category = new Category { CategoryId = 1, CategoryName = "Plumbing" };
            var mockFiles = new List<IFormFile>();

            _mockPropertyCategoryRepository.Setup(pc => pc.GetById(It.IsAny<int>())).ReturnsAsync(new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1, Category = category });
            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(false);

            var result = await _blobController.UploadBlob(mockFiles, propertyCategory);

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

            var result = await _blobController.DeleteBlob(document.DocumentId) as JsonResult;

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.That(result.Value.ToString().Equals("{ success = True }"));
        }
        
        [Test]
        public async Task DeleteBlob_FileDataNotExists_Fail()
        {
            var document = new Document();

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _blobController.DeleteBlob(document.DocumentId);

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Detail.Equals("The File MetaData was not found"));
            Assert.That(problemDetails.Title.Equals("Metadata Not Found"));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
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
            Assert.That(problemDetails.Detail.Equals("An error occurred when deleting the file"));
            Assert.That(problemDetails.Title.Equals("Deletion Error"));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status500InternalServerError));
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
            Assert.That(result.ViewName.Equals("_PDFViewer"));
            Assert.That(result.Model.Equals(blobUrl));
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
            Assert.That(result.ViewName.Equals("_VideoPlayer"));
            Assert.That(result.Model.Equals(blobUrl));
        }

        [Test]
        public async Task RenderFile_ImageExists_Success()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.jpg",
                ContentType = "image/jpg"
            };

            var blobUrl = "https://devstoreaccount1/test/category/test.jpg";

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(blobUrl);

            var result = await _blobController.RenderFile(documentId) as PartialViewResult;

            _mockBlobService.Verify(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.That(result.ViewName.Equals("_ImageViewer"));
            Assert.That(result.Model.Equals(blobUrl));
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
            Assert.That(problemDetails.Detail.Equals("The file was not found in blob storage"));
            Assert.That(problemDetails.Title.Equals("File Not Found"));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task Download_DocumentNotFound()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.txt"
            };

            _mockDocumentRepository.Setup(repo => repo.GetById(document.DocumentId))
            .ReturnsAsync((Document)null);

            var result = await _blobController.Download(document.DocumentId);

            _mockDocumentRepository.Verify(repo => repo.GetById(documentId), Times.Once);
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.AreEqual("Document Not Found", problemDetails.Title);
            Assert.AreEqual("The Document was not found", problemDetails.Detail);
            Assert.AreEqual(StatusCodes.Status404NotFound, problemDetails.Status);
        }

        [Test]
        public async Task Download_BlobNotFound()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.txt"
            };

            _mockDocumentRepository.Setup(repo => repo.GetById(documentId))
                .ReturnsAsync(document);

            _mockBlobService.Setup(service => service.DownloadBlobAsync("test1", document.BlobName))
                .ReturnsAsync((Stream)null);

            var result = await _blobController.Download(documentId);

            _mockDocumentRepository.Verify(repo => repo.GetById(documentId), Times.Once);
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.AreEqual("Document Not Found in Blob Storage", problemDetails.Title);
            Assert.AreEqual("Document Not Found in the Blob Storage", problemDetails.Detail);
            Assert.AreEqual(StatusCodes.Status404NotFound, problemDetails.Status);
        }

        [Test]
        public async Task Download_Success()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.txt",
                ContentType = "text/plain",
                FileName = "test.txt"
            };
            var ms = new MemoryStream();

            _mockDocumentRepository.Setup(repo => repo.GetById(documentId))
            .ReturnsAsync(document);

            _mockBlobService.Setup(service => service.DownloadBlobAsync("test1", document.BlobName))
                .ReturnsAsync(ms);

            var result = await _blobController.Download(documentId);
        }



        [TearDown]
        public void Teardown()
        {
            _blobController.Dispose();
        }
    }
}

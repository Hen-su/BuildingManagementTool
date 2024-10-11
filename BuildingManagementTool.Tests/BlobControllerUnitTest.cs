using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class BlobControllerUnitTest
    {
        private Mock<IBlobService> _mockBlobService;
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private Mock<IPropertyRepository> _mockPropertyRepository;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<IUserPropertyRepository> _mockUserPropertyRepository;
        private Mock<IAuthorizationService> _mockAuthorizationService;
        private BlobController _blobController;

        [SetUp]
        public void Setup()
        {
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockBlobService = new Mock<IBlobService>();
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockUserPropertyRepository = new Mock<IUserPropertyRepository>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
            _mockAuthorizationService = new Mock<IAuthorizationService>();
            _blobController = new BlobController(_mockBlobService.Object, _mockDocumentRepository.Object, _mockPropertyCategoryRepository.Object, 
                _mockUserManager.Object, _mockCategoryRepository.Object, _mockPropertyRepository.Object, _mockUserPropertyRepository.Object,
                _mockAuthorizationService.Object);
        }
        /*
        [Test]
        public async Task UploadBlob_ValidInputSingle_Success()
        {
            var propertyCategoryId = 1;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1, Property = property, Category = category };
            var content = "Hello World";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(1024);
            formFile.Setup(f => f.FileName).Returns("testfile.txt");
            formFile.Setup(f => f.ContentType).Returns("text/plain");
            formFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file content.")));
            formFile.Setup(f => f.Headers).Returns(new HeaderDictionary());

            var mockFiles = new List<IFormFile> { formFile.Object };

            _mockPropertyCategoryRepository.Setup(pc => pc.GetById(It.IsAny<int>())).ReturnsAsync(propertyCategory);

            _mockBlobService.Setup(x => x.BlobExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((bool)false);

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(true);

            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var result = await _blobController.UploadBlob(mockFiles, propertyCategoryId) as JsonResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Once);
            Assert.That(result.Value.ToString().Equals("{ success = True }"));
        }
    
    
        [Test]
        public async Task UploadBlob_ValidInputMultiple_Success()
        {
            var propertyCategoryId = 1;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1, Property = property, Category = category };
            var content = "Hello World";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(1024);
            formFile.Setup(f => f.FileName).Returns("testfile.txt");
            formFile.Setup(f => f.ContentType).Returns("text/plain");
            formFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file content.")));
            formFile.Setup(f => f.Headers).Returns(new HeaderDictionary());

            var mockFiles = new List<IFormFile> { formFile.Object, formFile.Object };

            _mockPropertyCategoryRepository.Setup(pc => pc.GetById(It.IsAny<int>())).ReturnsAsync(propertyCategory);

            _mockBlobService.Setup(x => x.BlobExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((bool)false);

            _mockBlobService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .ReturnsAsync(true);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var result = await _blobController.UploadBlob(mockFiles, propertyCategoryId) as JsonResult;

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Exactly(2));
            Assert.That(result.Value.ToString().Equals("{ success = True }"));
        }
        
        [Test]
        public async Task UploadBlob_NoInput_Fail()
        {
            var propertyCategoryId = 1;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var mockFiles = new List<IFormFile>();

            _mockPropertyCategoryRepository.Setup(pc => pc.GetById(It.IsAny<int>())).ReturnsAsync(new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1, Category = category });
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.UploadBlob(mockFiles, propertyCategoryId);

            _mockBlobService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()), Times.Exactly(0));
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteBlob_FileExists_Success()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.txt",
                PropertyCategory = propertyCategory
            };

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockBlobService.Setup(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.DeleteBlob(document.DocumentId) as JsonResult;

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.That(result.Value.ToString().Equals("{ success = True }"));
        }
        
        [Test]
        public async Task DeleteBlob_FileDataNotExists_Fail()
        {
            var document = new Document();

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockBlobService.Setup(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _blobController.DeleteBlob(document.DocumentId);

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }
        
        [Test]
        public async Task DeleteBlob_BlobDeleteFailed_Fail()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "category/test.txt",
                PropertyCategory = propertyCategory
            };

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.DeleteBlob(document.DocumentId);

            _mockBlobService.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status500InternalServerError), "Expected 500 Internal Server Error status code.");
        }
        
        [Test]
        public async Task RenderFile_PDFExists_Success()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.pdf",
                ContentType = "application/pdf",
                PropertyCategory = propertyCategory
            };

            var blobUrl = "https://devstoreaccount1/test/category/test.pdf";

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

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
            var propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.pdf",
                ContentType = "video/mp4",
                PropertyCategory = propertyCategory
            };

            var blobUrl = "https://devstoreaccount1/test/category/test.mp4";

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

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
            var propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.jpg",
                ContentType = "image/jpg",
                PropertyCategory = propertyCategory
            };

            var blobUrl = "https://devstoreaccount1/test/category/test.jpg";

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

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

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string) null);

            var result = await _blobController.RenderFile(documentId);
            _mockBlobService.Verify(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task RenderFile_FileNotExists_Fail()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.pdf",
                ContentType = "video/mp4",
                PropertyCategory = propertyCategory
            };

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockDocumentRepository.Setup(s => s.GetById(1)).ReturnsAsync(document);
            _mockBlobService.Setup(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string)null);

            var result = await _blobController.RenderFile(documentId);
            _mockBlobService.Verify(s => s.GetBlobUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task Download_DocumentNotFound()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.txt"
            };

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockDocumentRepository.Setup(repo => repo.GetById(document.DocumentId))
            .ReturnsAsync((Document)null);

            var result = await _blobController.Download(document.DocumentId);

            _mockDocumentRepository.Verify(repo => repo.GetById(documentId), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task Download_BlobNotFound()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1 };
            var managerId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var role = new IdentityRole() { Id = roleId, Name = "Manager" };
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, Property = property, UserId = managerId, RoleId = role.Id, Role = role };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.txt",
                ContentType = "text/plain",
                FileName = "test.txt",
                PropertyCategory = propertyCategory
            };
            var ms = new MemoryStream();

            _mockDocumentRepository.Setup(repo => repo.GetById(documentId))
            .ReturnsAsync(document);

            _mockUserPropertyRepository.Setup(u => u.GetManagerUserIdByPropertyId(It.IsAny<int>())).ReturnsAsync(managerId);

            _mockBlobService.Setup(service => service.DownloadBlobAsync("test1", document.BlobName))
                .ReturnsAsync((Stream)null);

            var result = await _blobController.Download(documentId);

            _mockDocumentRepository.Verify(repo => repo.GetById(documentId), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task Download_Success()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1 };
            var managerId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var role = new IdentityRole() { Id = roleId, Name = "Manager" };
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, Property = property, UserId = managerId, RoleId = role.Id, Role = role };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.txt",
                ContentType = "text/plain",
                FileName = "test.txt",
                PropertyCategory = propertyCategory
            };
            var ms = new MemoryStream();

            _mockDocumentRepository.Setup(repo => repo.GetById(documentId))
            .ReturnsAsync(document);

            _mockUserPropertyRepository.Setup(u => u.GetManagerUserIdByPropertyId(It.IsAny<int>())).ReturnsAsync(managerId);

            _mockBlobService.Setup(service => service.DownloadBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ms);

            var result = await _blobController.Download(documentId) as FileStreamResult;
            Assert.NotNull(result);
            Assert.That(result.ContentType.Equals(document.ContentType));
            Assert.That(result.FileDownloadName.Equals(document.FileName));
        }

        [Test]
        public async Task DeletePropertyCategory_DeleteSuccess_ReturnSuccess()
        {
            var propertyCategoryId = 1;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1, Property = property, Category = category };
            
            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(propertyCategoryId))
            .ReturnsAsync(propertyCategory);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockBlobService.Setup(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.DeletePropertyCategory(propertyCategoryId);
            _mockBlobService.Verify(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task DeletePropertyCategory_DeleteFailed_ReturnError()
        {
            var propertyCategoryId = 1;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1, Property = property, Category = category };

            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(propertyCategoryId))
            .ReturnsAsync(propertyCategory);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockBlobService.Setup(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.DeletePropertyCategory(propertyCategoryId);

            _mockBlobService.Verify(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status500InternalServerError), "Expected 500 Internal Server Error status code.");
        }

        [Test]
        public async Task GetDocumentShareUrl_DocumentNotFound()
        {
            var documentId = 1;
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.txt"
            };
            _mockDocumentRepository.Setup(repo => repo.GetById(document.DocumentId))
            .ReturnsAsync((Document)null);

            var result = await _blobController.GetDocumentShareUrl(document.DocumentId);

            _mockDocumentRepository.Verify(repo => repo.GetById(documentId), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }
  
        [Test]
        public async Task RenameCategoryPropertyCategory_ValidId_ReturnSuccess()
        {
            var propertyCategoryId = 1;
            var newCategory = "newCategory";
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1, Property = property, Category = category };

            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(propertyCategoryId))
            .ReturnsAsync(propertyCategory);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockPropertyCategoryRepository.Setup(repo => repo.UpdatePropertyCategory(It.IsAny<PropertyCategory>())).Returns(Task.CompletedTask);

            var documentList = new List<Document>
            { 
                new Document { DocumentId = 1, FileName = "text.txt", BlobName = "property/category/text.txt", PropertyCategoryId = 1 },
                new Document { DocumentId = 2, FileName = "text2.txt", BlobName = "property/category/text2.txt", PropertyCategoryId = 1}
            };

            _mockDocumentRepository.Setup(d => d.GetByPropertyCategoryId(It.IsAny<int>())).ReturnsAsync(documentList);

            _mockDocumentRepository.Setup(d => d.UpdateDocumentAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);

            _mockBlobService.Setup(b => b.RenameBlobDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.RenamePropertyCategory(propertyCategoryId, newCategory);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task RenameCategoryPropertyCategory_NullId_ReturnError()
        {
            var propertyCategoryId = 1;
            PropertyCategory propertyCategory = null;
            var newCategory = "newCategory";
            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(propertyCategoryId))
            .ReturnsAsync(propertyCategory);

            var result = await _blobController.RenamePropertyCategory(propertyCategoryId, newCategory);
            _mockPropertyCategoryRepository.Verify(d => d.UpdatePropertyCategory(It.IsAny<PropertyCategory>()), Times.Never);
            _mockDocumentRepository.Verify(d => d.UpdateDocumentAsync(It.IsAny<Document>()), Times.Never);
            _mockBlobService.Verify(b => b.RenameBlobDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }


        [Test]
        public async Task RenameCategoryPropertyCategory_NullName_ReturnError()
        {
            var propertyCategoryId = 1;
            string newCategory = null;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1, Property = property, Category = category };
            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(propertyCategoryId))
            .ReturnsAsync(propertyCategory);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.RenamePropertyCategory(propertyCategoryId, newCategory);
            _mockPropertyCategoryRepository.Verify(d => d.UpdatePropertyCategory(It.IsAny<PropertyCategory>()), Times.Never);
            _mockDocumentRepository.Verify(d => d.UpdateDocumentAsync(It.IsAny<Document>()), Times.Never);
            _mockBlobService.Verify(b => b.RenameBlobDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status400BadRequest), "Expected 400 Bad Request status code.");
        }

        [Test]
        public async Task DeleteProperty_DeleteSuccess_ReturnSuccess()
        {
            var propertyId = 1;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
           
            _mockPropertyRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync(property);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockBlobService.Setup(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.DeleteProperty(propertyId);
            _mockBlobService.Verify(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task DeleteProperty_PropertyNotFound_ReturnError()
        {
            var propertyId = 1;

            _mockPropertyRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync((Property)null);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockBlobService.Setup(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = await _blobController.DeleteProperty(propertyId);
            _mockBlobService.Verify(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task GetDocumentShareUrl_Success()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1 };
            var managerId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var role = new IdentityRole() { Id = roleId, Name = "Manager" };
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, Property = property, UserId = managerId, RoleId = role.Id, Role = role };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.txt",
                ContentType = "text/plain",
                FileName = "test.txt",
                PropertyCategory = propertyCategory
            };
            var blobUrl = "https://devstoreaccount1/test/category/test.pdf";

            _mockDocumentRepository.Setup(repo => repo.GetById(documentId))
                .ReturnsAsync(document);

            _mockUserPropertyRepository.Setup(u => u.GetManagerUserIdByPropertyId(It.IsAny<int>())).ReturnsAsync(managerId);

            _mockBlobService.Setup(service => service.GetBlobUrlAsync(It.IsAny<string>(), document.BlobName))
                .ReturnsAsync(blobUrl);

            var result = await _blobController.GetDocumentShareUrl(documentId) as OkObjectResult;

            Assert.NotNull(result);
           
            var resultJson = JsonConvert.SerializeObject(result.Value); 
            var resultValue = JObject.Parse(resultJson);  

            Assert.IsTrue(resultValue["success"].Value<bool>(), "Expected 'success' to be true.");
            Assert.That(resultValue["url"].Value<string>(), Is.EqualTo(blobUrl), "Expected 'url' to match the blob URL.");
        }

        [Test]
        public async Task GetDocumentShareUrl_BlobNotFound()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1 };
            var managerId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var role = new IdentityRole() { Id = roleId, Name = "Manager" };
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, Property = property, UserId = managerId, RoleId = role.Id, Role = role };
            var document = new Document
            {
                DocumentId = documentId,
                BlobName = "property/category/test.txt",
                ContentType = "text/plain",
                FileName = "test.txt",
                PropertyCategory = propertyCategory
            };

            _mockDocumentRepository.Setup(repo => repo.GetById(documentId))
                .ReturnsAsync(document);

            _mockUserPropertyRepository.Setup(u => u.GetManagerUserIdByPropertyId(It.IsAny<int>())).ReturnsAsync(managerId);

            _mockBlobService.Setup(service => service.GetBlobUrlAsync("test1", document.BlobName))
                .ReturnsAsync((string)null);

            var result = await _blobController.GetDocumentShareUrl(documentId);

            _mockDocumentRepository.Verify(repo => repo.GetById(documentId), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task DeleteProperty_DeleteFailed_ReturnError()
        {
            var propertyId = 1;
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };

            _mockPropertyRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync(property);

            var user = new ApplicationUser { Id = "4b5a1f27-df5d-4b8e-85a3-3fabc47d5e9a" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockBlobService.Setup(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _blobController.DeleteProperty(propertyId);
            _mockBlobService.Verify(b => b.DeleteByPrefix(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status500InternalServerError), "Expected 500 Internal Server Error status code.");
        }
        */
        [TearDown]
        public void Teardown()
        {
            _blobController.Dispose();
        }
    }
}

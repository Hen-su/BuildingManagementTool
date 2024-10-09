using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class UserPropertyControllerUnitTest
    {
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private Mock<IPropertyRepository> _mockPropertyRepository;
        private Mock<IUserPropertyRepository> _mockUserPropertyRepository;
        private Mock<IUserStore<ApplicationUser>> _mockUserStore;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private Mock<IBlobService> _mockBlobService;
        private Mock<IPropertyImageRepository> _mockPropertyImageRepository;
        private Mock<IInvitationService> _mockInvitationService;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IRoleStore<IdentityRole>> _mockRoleStore;
        private Mock<IOptions<IdentityOptions>> _mockOptions;
        private Mock<IRazorViewToStringRenderer> _renderer;
        private Mock<IEmailSender> _mockEmailSender;
        private Mock<IAuthorizationService> _mockAuthorizationService;

        private UserPropertyController _userPropertyController;

        [SetUp]
        public void Setup()
        {
            _mockUserPropertyRepository = new Mock<IUserPropertyRepository>();
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(_mockUserStore.Object, null, null, null, null, null, null, null, null);
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockBlobService = new Mock<IBlobService>();
            _mockPropertyImageRepository = new Mock<IPropertyImageRepository>();
            _mockInvitationService = new Mock<IInvitationService>();
            _mockAuthorizationService = new Mock<IAuthorizationService>();

        var list = new List<IdentityRole>()
            {
                new IdentityRole("Administrator"),
                new IdentityRole("Visitor")
            }
            .AsQueryable();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object,
                new IRoleValidator<IdentityRole>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

            _userPropertyController = new UserPropertyController(_mockUserPropertyRepository.Object, _mockPropertyRepository.Object, 
                _mockUserManager.Object, _mockDocumentRepository.Object, _mockPropertyCategoryRepository.Object, _mockBlobService.Object, 
                _mockPropertyImageRepository.Object, _mockRoleManager.Object, _mockInvitationService.Object, _mockAuthorizationService.Object);
        }

        [Test]
        public async Task Index_ValidUser_ReturnView()
        {
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email};
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockRoleManager.Setup(r => r.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            var propertyList = new List<UserProperty>
            {
                new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId },
                new UserProperty { UserPropertyId = 2, PropertyId = 2, UserId = userId }
            };
            _mockUserPropertyRepository.Setup(u => u.GetByUserId(It.IsAny<string>())).ReturnsAsync(propertyList);

            var result = await _userPropertyController.Index();
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }

        [Test]
        public async Task Index_InvalidUser_ReturnError()
        {
            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

            var result = await _userPropertyController.Index();
            var objectResult = result as ObjectResult;
            Assert.IsInstanceOf<BadRequestObjectResult>(objectResult);
            Assert.AreEqual("A problem occurred while retrieving your data", objectResult.Value);
        }

        [Test]
        public async Task PropertyFormPartial_NullCurrentProperty_ReturnPartial()
        {
            var viewModel = new PropertyFormViewModel(null);
            var result = await _userPropertyController.PropertyFormPartial();
            var viewResult = result as PartialViewResult;
            Assert.IsNotNull(viewResult);
            Assert.That(viewResult.ViewName, Is.EqualTo("_PropertyForm"));
        }

        [Test]
        public async Task AddProperty_ValidName_ReturnSuccess()
        {
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockRoleManager.Setup(r => r.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(role);
            var name = "Example Property";

            _mockPropertyRepository.Setup(p => p.AddProperty(It.IsAny<Models.Property>())).Returns(Task.CompletedTask);
            _mockUserPropertyRepository.Setup(u => u.AddUserProperty(It.IsAny<UserProperty>())).Returns(Task.CompletedTask);
            _mockPropertyRepository.Setup(p => p.AddDefaultCategories(It.IsAny<Models.Property>())).Returns(Task.CompletedTask);

            var result = await _userPropertyController.AddProperty(name);
            
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task AddProperty_NullName_ReturnSuccess()
        {
            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            string name = null;

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var result = await _userPropertyController.AddProperty(name);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdatePropertyContainer_ValidUser_ReturnPartial()
        {
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockRoleManager.Setup(r => r.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            var propertyList = new List<UserProperty>
            {
                new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId },
                new UserProperty { UserPropertyId = 2, PropertyId = 2, UserId = userId }
            };
            _mockUserPropertyRepository.Setup(u => u.GetByUserId(It.IsAny<string>())).ReturnsAsync(propertyList);

            var result = await _userPropertyController.UpdatePropertyContainer();
            var viewResult = result as PartialViewResult;
            Assert.IsNotNull(viewResult);
            Assert.That(viewResult.ViewName, Is.EqualTo("_PropertyContainer"));
        }

        [Test]
        public async Task UpdatePropertyContainer_InvalidUser_ReturnError()
        {
            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

            var result = await _userPropertyController.UpdatePropertyContainer();
            var objectResult = result as ObjectResult;
            Assert.IsInstanceOf<BadRequestObjectResult>(objectResult);
            Assert.AreEqual("A problem occurred while retrieving your data", objectResult.Value);
        }

        [Test]
        public async Task DeleteConfirmationPartial_ValidId_ReturnPartial()
        {
            int id = 1;
            var property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            _mockPropertyRepository.Setup(p => p.GetById(It.IsAny<int>())).ReturnsAsync(property);

            var result = await _userPropertyController.DeleteConfirmationPartial(id);
            var viewResult = result as PartialViewResult;
            Assert.IsNotNull(viewResult);
            Assert.That(viewResult.ViewName, Is.EqualTo("_PropertyDeleteConfirmation"));
        }

        [Test]
        public async Task DeleteConfirmationPartial_NullId_ReturnError()
        {
            int id = 0;
            var property = new Models.Property();
            _mockPropertyRepository.Setup(p => p.GetById(It.IsAny<int>())).ReturnsAsync(property);

            var result = await _userPropertyController.DeleteConfirmationPartial(id);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status400BadRequest), "Expected 400 Bad Request status code.");
        }

        [Test]
        public async Task DeleteConfirmationPartial_PropertyNotExist_ReturnError()
        {
            int id = 1;
            _mockPropertyRepository.Setup(p => p.GetById(It.IsAny<int>())).ReturnsAsync((Models.Property) null);

            var result = await _userPropertyController.DeleteConfirmationPartial(id);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task ManagePropertyFormPartial_Valid_ReturnPartial()
        {
            int id = 1;
            var property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var imageList = new List<Dictionary<int, List<string>>>();
            var dbImages = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", IsDisplay = false, PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", IsDisplay = false, PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image3.jpg", BlobName = "user/property/images/image3.jpg", IsDisplay = false, PropertyId = 1},
                new PropertyImage {Id = 3, FileName = "image4.jpg", BlobName = "user/property/images/image4.jpg", IsDisplay = false, PropertyId = 1}
            };
            var returnList = new List<Dictionary<int, List<string>>>()
            {
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image1.jpg", "Test Property/images/image1.jpg" } }
                },
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image2.jpg", "Test Property/images/image2.jpg" } }
                },
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image3.jpg", "Test Property/images/image3.jpg" } }
                },
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image4.jpg", "Test Property/images/image4.jpg" } }
                }
            };

            Dictionary<int, List<string>> blobList = new Dictionary<int, List<string>>
            {
                { 0, new List<string> { "image1.jpg", "Test Property/images/image1.jpg" } },
                { 1, new List<string> { "image2.jpg", "Test Property/images/image2.jpg" } },
                { 2, new List<string> { "image3.jpg", "Test Property/images/image3.jpg" } },
                { 3, new List<string> { "image4.jpg", "Test Property/images/image4.jpg" } }
            };

            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockPropertyRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(property);
            _mockPropertyImageRepository.Setup(x => x.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(dbImages);
            _mockBlobService.Setup(x => x.GetBlobUrisByPrefix(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(blobList);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _userPropertyController.ManagePropertyFormPartial(id);
            var viewResult = result as PartialViewResult;
            Assert.IsNotNull(viewResult);
            Assert.That(viewResult.ViewName, Is.EqualTo("_ManagePropertyForm"));
        }

        [Test]
        public async Task ManagePropertyFormPartial_NullPropertyId_ReturnError()
        {
            int id = 0;
            Models.Property property = null;
            
            var result = await _userPropertyController.ManagePropertyFormPartial(id);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status400BadRequest), "Expected 400 Bad Request status code.");
        }

        [Test]
        public async Task ManagePropertyFormPartial_NotFoundPropertyId_ReturnError()
        {
            int id = 1;
            Models.Property property = null;
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);
            var result = await _userPropertyController.ManagePropertyFormPartial(id);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status500InternalServerError), "Expected 500 Internal Server Error status code.");
        }

        

        [Test]
        public async Task ManagePropertyFormSubmit_ValidPropertyName_ReturnSuccess()
        {
            int id = 1;
            ManagePropertyFormViewModel viewModel = new ManagePropertyFormViewModel { PropertyName = "New Name" };
            var property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var document = new List<Document> { new Document { DocumentId = 1, FileName = "Document 1", BlobName = "Test Property/Category/Document 1.docx" } };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockPropertyRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(property);
            _mockDocumentRepository.Setup(x => x.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(document);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _userPropertyController.ManagePropertyFormSubmit(id, viewModel, null);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task ManagePropertyFormSubmit_ImageUpload_ReturnSuccess()
        {
            int id = 1;
            
            var property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var document = new List<Document> { new Document { DocumentId = 1, FileName = "Document 1", BlobName = "Test Property/Category/Document 1.docx" } };
            
            var fileMock1 = new Mock<IFormFile>();
            var fileName1 = "image1.jpg";
            var fileContent1 = new byte[] { 255, 216, 255, 224, 0, 16, 74, 70, 73, 70 }; // Sample binary data for a JPEG image header
            var memoryStream1 = new MemoryStream(fileContent1);

            fileMock1.Setup(f => f.FileName).Returns(fileName1);
            fileMock1.Setup(f => f.Length).Returns(memoryStream1.Length);
            fileMock1.Setup(f => f.OpenReadStream()).Returns(memoryStream1);
            fileMock1.Setup(f => f.ContentDisposition).Returns($"form-data; name=\"{fileName1}\"; filename=\"{fileName1}\"");
            fileMock1.Setup(f => f.ContentType).Returns("image/jpeg");

            var files = new List<IFormFile>
            {
                fileMock1.Object
            };

            ManagePropertyFormViewModel viewModel = new ManagePropertyFormViewModel { PropertyName = "New Name", Images = files };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockPropertyRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(property);
            _mockDocumentRepository.Setup(x => x.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(document);
            _mockPropertyImageRepository.Setup(x => x.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(new List<PropertyImage>());
            _mockBlobService.Setup(x => x.BlobExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((bool)false);
            _mockBlobService.Setup(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>())).ReturnsAsync((bool)true);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _userPropertyController.ManagePropertyFormSubmit(id, viewModel, null);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task ManagePropertyFormSubmit_SetDisplay_ReturnSuccess()
        {
            int id = 1;
            string selectedFile = "image1.jpg";
            PropertyImage propertyImage = new PropertyImage { Id = 1, FileName = "image1.jpg", BlobName = "Test Property/images/image1.jpg", IsDisplay = false, PropertyId = 1 };
            var property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var document = new List<Document> { new Document { DocumentId = 1, FileName = "Document 1", BlobName = "Test Property/Category/Document 1.docx" } };

            ManagePropertyFormViewModel viewModel = new ManagePropertyFormViewModel { PropertyName = "New Name" };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockPropertyRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(property);
            _mockDocumentRepository.Setup(x => x.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(document);
            _mockPropertyImageRepository.Setup(x => x.GetByFileName(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(propertyImage);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _userPropertyController.ManagePropertyFormSubmit(id, viewModel, selectedFile);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task SendInviteEmail_ValidModelState_ReturnTrue()
        {
            var id = 1;
            var addViewerViewModel = new AddViewerViewModel
            {
                Email = "test@example.com"
            };
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _userPropertyController.SendInviteEmail(id, addViewerViewModel) as JsonResult;

            _mockInvitationService.Verify(s => s.InviteUserAsync(addViewerViewModel.Email, id), Times.Once);
            Assert.NotNull(result);
            Assert.IsTrue((bool)result.Value.GetType().GetProperty("success").GetValue(result.Value));
        }

        [Test]
        public async Task SendInviteEmail_ModelStateInvalid_ReturnPartial()
        {
            var id = 1;

            var property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var imageList = new List<Dictionary<int, List<string>>>();
            var dbImages = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", IsDisplay = false, PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", IsDisplay = false, PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image3.jpg", BlobName = "user/property/images/image3.jpg", IsDisplay = false, PropertyId = 1},
                new PropertyImage {Id = 3, FileName = "image4.jpg", BlobName = "user/property/images/image4.jpg", IsDisplay = false, PropertyId = 1}
            };
            var returnList = new List<Dictionary<int, List<string>>>()
            {
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image1.jpg", "Test Property/images/image1.jpg" } }
                },
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image2.jpg", "Test Property/images/image2.jpg" } }
                },
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image3.jpg", "Test Property/images/image3.jpg" } }
                },
                new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "image4.jpg", "Test Property/images/image4.jpg" } }
                }
            };

            Dictionary<int, List<string>> blobList = new Dictionary<int, List<string>>
            {
                { 0, new List<string> { "image1.jpg", "Test Property/images/image1.jpg" } },
                { 1, new List<string> { "image2.jpg", "Test Property/images/image2.jpg" } },
                { 2, new List<string> { "image3.jpg", "Test Property/images/image3.jpg" } },
                { 3, new List<string> { "image4.jpg", "Test Property/images/image4.jpg" } }
            };

            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockPropertyRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(property);
            _mockPropertyImageRepository.Setup(x => x.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(dbImages);
            _mockBlobService.Setup(x => x.GetBlobUrisByPrefix(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(blobList);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var addViewerViewModel = new AddViewerViewModel
            {
                Email = "invalidEmail"
            };

            _userPropertyController.ModelState.AddModelError("Email", "The Email field is required.");

            var result = await _userPropertyController.SendInviteEmail(id, addViewerViewModel) as PartialViewResult;

            Assert.NotNull(result);
            Assert.AreEqual("_ManagePropertyForm", result.ViewName);
        }

        [Test]
        public async Task DeleteUserProperty_IdAndEmailValid_ReturnSuccess()
        {
            int id = 1;
            var userId = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var userProperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = roleId };

            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserPropertyRepository.Setup(u => u.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userProperty);
            _mockUserPropertyRepository.Setup(u => u.DeleteUserProperty(It.IsAny<UserProperty>())).Returns(Task.CompletedTask);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _userPropertyController.DeleteUserProperty(id, email) as JsonResult;
            Assert.NotNull(result);
            Assert.IsTrue((bool)result.Value.GetType().GetProperty("success").GetValue(result.Value));
        }

        [Test]
        public async Task DeleteUserProperty_IdInvalid_ReturnError()
        {
            int id = 0;
            var email = "example@example.com";
            var result = await _userPropertyController.DeleteUserProperty(id, email) as ObjectResult;
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status400BadRequest), "Expected 400 Bad Request status");
        }

        [Test]
        public async Task DeleteUserProperty_EmailInvalid_ReturnError()
        {
            int id = 1;
            var email = (string) null;
            var result = await _userPropertyController.DeleteUserProperty(id, email) as ObjectResult;
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status400BadRequest), "Expected 400 Bad Request status");
        }

        [TearDown]
        public void Teardown()
        {
            _userPropertyController.Dispose();
        }
    }
}

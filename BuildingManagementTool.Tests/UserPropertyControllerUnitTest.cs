using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        private UserPropertyController _userPropertyController;

        [SetUp]
        public void Setup()
        {
            _mockUserPropertyRepository = new Mock<IUserPropertyRepository>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(_mockUserStore.Object, null, null, null, null, null, null, null, null);
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockBlobService = new Mock<IBlobService>();
            _mockPropertyImageRepository = new Mock<IPropertyImageRepository>();

            _userPropertyController = new UserPropertyController(_mockUserPropertyRepository.Object, _mockPropertyRepository.Object, 
                _mockUserManager.Object, _mockDocumentRepository.Object, _mockPropertyCategoryRepository.Object, _mockBlobService.Object, _mockPropertyImageRepository.Object);
        }

        [Test]
        public async Task Index_ValidUser_ReturnView()
        {
            var userId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email};

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
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
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

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
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };

            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
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


        [TearDown]
        public void Teardown()
        {
            _userPropertyController.Dispose();
        }
    }
}

using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class PropertyCategoryControllerUnitTest
    {
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private Mock<IPropertyRepository> _mockPropertyRepository;
        private Mock<IUserPropertyRepository> _mockUserPropertyRepository;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private Mock<IPropertyImageRepository> _mockPropertyImageRepository;
        private Mock<IBlobService> _mockBlobService;

        private Mock<IUserStore<ApplicationUser>> _mockUserStore;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IRoleStore<IdentityRole>> _mockRoleStore;
        private Mock<IAuthorizationService> _mockAuthorizationService;

        private PropertyCategoryController _propertyCategoryController;
        

        [SetUp]
        public void Setup()
        {
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockUserPropertyRepository = new Mock<IUserPropertyRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockPropertyImageRepository = new Mock<IPropertyImageRepository>();
            _mockBlobService = new Mock<IBlobService>();
            _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(_mockUserStore.Object, null, null, null, null, null, null, null, null);
            _mockAuthorizationService = new Mock<IAuthorizationService>();

            _propertyCategoryController = new PropertyCategoryController(_mockPropertyCategoryRepository.Object,
                                                                         _mockPropertyRepository.Object,
                                                                         _mockCategoryRepository.Object,
                                                                         _mockDocumentRepository.Object,
                                                                         _mockPropertyImageRepository.Object,
                                                                         _mockUserManager.Object,
                                                                         _mockBlobService.Object,
                                                                         _mockAuthorizationService.Object, 
                                                                         _mockUserPropertyRepository.Object);
        }

        [Test]
        public async Task Index_PropertyExists_ReturnView()
        {
            int id = 1;
            var list = new List<PropertyCategory>
            {
                new PropertyCategory{ PropertyCategoryId = 1 ,PropertyId = 1, CategoryId = 1 },
                new PropertyCategory{ PropertyCategoryId = 2 ,PropertyId = 1, CategoryId = 2 },
                new PropertyCategory{ PropertyCategoryId = 3 ,PropertyId = 2, CategoryId = 1 },
            };
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = role.Id, Role = role };
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserPropertyRepository.Setup(x => x.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);
            _mockPropertyCategoryRepository.Setup(pc => pc.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(list.Where(p => p.PropertyId == id));
            _mockPropertyImageRepository.Setup(pi => pi.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(new List<PropertyImage>());

            var result = await _propertyCategoryController.Index(id);
            var viewResult = (ViewResult) result;
            var resultViewModel = viewResult.Model as CategoryViewModel;
            Assert.IsNotNull(result);
            Assert.That(resultViewModel.PropertyCategories.Count().Equals(2));
        }
        
        [Test]
        public async Task Index_PropertyNotExists_ReturnError()
        {
            int id = 1;
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };
            var userproperty = (UserProperty)null;
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserPropertyRepository.Setup(x => x.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);

            var result = await _propertyCategoryController.Index(id);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }
        
        [Test]
        public async Task UpdateCategoryCanvas_PropertyExists_ReturnPartial()
        {
            int id = 1;
            var list = new List<PropertyCategory>
            {
                new PropertyCategory{ PropertyCategoryId = 1 ,PropertyId = 1, CategoryId = 1 },
                new PropertyCategory{ PropertyCategoryId = 2 ,PropertyId = 1, CategoryId = 2 },
                new PropertyCategory{ PropertyCategoryId = 3 ,PropertyId = 2, CategoryId = 1 },
            };
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = role.Id, Role = role };
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserPropertyRepository.Setup(x => x.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);
            _mockPropertyCategoryRepository.Setup(pc => pc.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(list.Where(p => p.PropertyId == id));
            _mockPropertyImageRepository.Setup(pi => pi.GetByPropertyId(It.IsAny<int>())).ReturnsAsync(new List<PropertyImage>());

            var result = await _propertyCategoryController.UpdateCategoryCanvas(id);
            var viewResult = result as PartialViewResult;
            var resultViewModel = viewResult.Model as CategoryViewModel;
            Assert.IsNotNull(result);
            Assert.That(viewResult.ViewName.Equals("_CategoryCanvas"));
            Assert.That(resultViewModel.PropertyCategories.Count().Equals(2));
        }
        
        [Test]
        public async Task UpdateCategoryCanvas_PropertyNotExists_ReturnError()
        {
            int id = 1;
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };
            var userproperty = (UserProperty)null;
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserPropertyRepository.Setup(x => x.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);

            var result = await _propertyCategoryController.UpdateCategoryCanvas(id);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task CategoryFormPartial_PropertyExists_ReturnPartial() 
        {
            int id = 1;
            Models.Property property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var list = new List<Category>
            {
                new Category{ CategoryId = 1, CategoryName = "Test1" },
                new Category{ CategoryId = 2, CategoryName = "Test2" },
                new Category{ CategoryId = 3, CategoryName = "Test3" }
            };
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);
            _mockCategoryRepository.Setup(pc => pc.Categories()).ReturnsAsync(list);
            var viewModel = new CategoryFormViewModel(list, id, null);

            var result = await _propertyCategoryController.CategoryFormPartial(id);
            var viewResult = result as PartialViewResult;
            var resultViewModel = viewResult.Model as CategoryFormViewModel;
            Assert.IsNotNull(result);
            Assert.That(viewResult.ViewName.Equals("_AddCategoryForm"));
            Assert.That(resultViewModel.Categories.Count().Equals(3));
        }

        [Test]
        public async Task CategoryFormPartial_PropertyNotExists_ReturnError()
        {
            int id = 1;
            Models.Property property = null;
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var result = await _propertyCategoryController.CategoryFormPartial(id);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task AddCategory_PropertyExists_ReturnSuccess()
        {
            int id = 1;
            Models.Property property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var list = new List<Category>
            {
                new Category{ CategoryId = 1, CategoryName = "Test1" },
                new Category{ CategoryId = 2, CategoryName = "Test2" },
                new Category{ CategoryId = 3, CategoryName = "Test3" }
            };
            string categoryName = "NewCategory";
            var viewModel = new CategoryFormViewModel { CategoryName = categoryName };
            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(list);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _propertyCategoryController.AddCategory(1, viewModel);
            _mockPropertyCategoryRepository.Verify(repo => repo.AddPropertyCategory(It.IsAny<PropertyCategory>()), Times.Once);

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task AddCategory_PropertyExists_ReturnError()
        {
            int id = 1;
            Models.Property property = null;
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var list = new List<Category>
            {
                new Category{ CategoryId = 1, CategoryName = "Test1" },
                new Category{ CategoryId = 2, CategoryName = "Test2" },
                new Category{ CategoryId = 3, CategoryName = "Test3" }
            };
            string categoryName = "NewCategory";
            var viewModel = new CategoryFormViewModel { CategoryName = categoryName };
            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(list);

            var result = await _propertyCategoryController.AddCategory(1, viewModel);
            _mockPropertyCategoryRepository.Verify(repo => repo.AddPropertyCategory(It.IsAny<PropertyCategory>()), Times.Never);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }
    
        [Test]
        public async Task DeleteConfirmationPartial_PropertyExists_ReturnPartial()
        {
            int id = 1;
            Models.Property property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            Models.Category category = new Models.Category { CategoryId = 1, CategoryName = "Test Category" };
            PropertyCategory propertyCategory = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1, Category = category, Property = property };
            _mockPropertyCategoryRepository.Setup(p => p.GetById(id)).ReturnsAsync(propertyCategory);

            var result = await _propertyCategoryController.DeleteConfirmationPartial(id);
            var viewResult = result as PartialViewResult;
            Assert.IsNotNull(result);
            Assert.That(viewResult.ViewName.Equals("_CategoryDeleteConfirmation"));
        }

        [Test]
        public async Task DeleteConfirmationPartial_PropertyNotExists_ReturnError()
        {
            int id = 1;
            Models.Property property = null;
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var result = await _propertyCategoryController.DeleteConfirmationPartial(id);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task DeleteCategory_PropertyExists_ReturnSuccess()
        {
            int id = 1;
            Models.Property property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var list = new List<Category>
            {
                new Category{ CategoryId = 1, CategoryName = "Test1" },
                new Category{ CategoryId = 2, CategoryName = "Test2" },
                new Category{ CategoryId = 3, CategoryName = "Test3" }
            };
            string categoryName = "NewCategory";
            var viewModel = new CategoryFormViewModel { CategoryName = categoryName };
            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(list);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _propertyCategoryController.AddCategory(1, viewModel);
            _mockPropertyCategoryRepository.Verify(repo => repo.AddPropertyCategory(It.IsAny<PropertyCategory>()), Times.Once);

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True }"));
        }

        [Test]
        public async Task DeleteCategory_PropertyExists_ReturnError()
        {
            int id = 1;
            Models.Property property = null;
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var list = new List<Category>
            {
                new Category{ CategoryId = 1, CategoryName = "Test1" },
                new Category{ CategoryId = 2, CategoryName = "Test2" },
                new Category{ CategoryId = 3, CategoryName = "Test3" }
            };
            string categoryName = "NewCategory";
            var viewModel = new CategoryFormViewModel { CategoryName = categoryName };
            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(list);

            var result = await _propertyCategoryController.AddCategory(1, viewModel);
            _mockPropertyCategoryRepository.Verify(repo => repo.AddPropertyCategory(It.IsAny<PropertyCategory>()), Times.Never);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task EditCategoryFormPartial_PropertyExists_ReturnPartial()
        {
            int id = 1;
            int currentCategoryId = 1;
            PropertyCategory propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1 };
            var list = new List<Category>
            {
                new Category{ CategoryId = 1, CategoryName = "Test1" },
                new Category{ CategoryId = 2, CategoryName = "Test2" },
                new Category{ CategoryId = 3, CategoryName = "Test3" }
            };
            Models.Property property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            _mockPropertyCategoryRepository.Setup(pc => pc.GetById(currentCategoryId)).ReturnsAsync(propertyCategory);

            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(list);

            var result = await _propertyCategoryController.EditCategoryFormPartial(id, currentCategoryId);
            var viewResult = result as PartialViewResult;
            Assert.That(viewResult.ViewName.Equals("_EditCategoryForm"));
            var resultViewModel = viewResult.Model as CategoryFormViewModel;
            Assert.That(resultViewModel.Categories.Count().Equals(3));
        }

        [Test]
        public async Task EditCategoryFormPartial_PropertyNotExists_ReturnError()
        {
            int id = 1;
            int currentCategoryId = 1;
            Models.Property property = null;
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var result = await _propertyCategoryController.EditCategoryFormPartial(id, currentCategoryId);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task RenameCategoryPropertyCategory_ValidId_ReturnSuccess()
        {
            var propertyCategoryId = 1;
            var newCategory = "newCategory";
            var property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1, Property = property, Category = category };
            
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };
            var role = new IdentityRole { Name = "Manager" };
            var userproperty = new UserProperty { UserId = Guid.NewGuid().ToString(), Role = role };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockUserPropertyRepository.Setup(up => up.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);
            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(propertyCategoryId))
            .ReturnsAsync(propertyCategory);
            _mockPropertyCategoryRepository.Setup(repo => repo.UpdatePropertyCategory(It.IsAny<PropertyCategory>())).Returns(Task.CompletedTask);

            var documentList = new List<Document>
            {
                new Document { DocumentId = 1, FileName = "text.txt", BlobName = "property/category/text.txt", PropertyCategoryId = 1 },
                new Document { DocumentId = 2, FileName = "text2.txt", BlobName = "property/category/text2.txt", PropertyCategoryId = 1}
            };

            var viewModel = new CategoryFormViewModel { CategoryName = "New Category" };
            _mockDocumentRepository.Setup(d => d.GetByPropertyCategoryId(It.IsAny<int>())).ReturnsAsync(documentList);

            _mockDocumentRepository.Setup(d => d.UpdateDocumentAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);

            _mockBlobService.Setup(b => b.RenameBlobDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _propertyCategoryController.RenamePropertyCategory(propertyCategoryId, viewModel);
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
            var viewModel = new CategoryFormViewModel { CategoryName = "New Category" };

            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(propertyCategoryId))
            .ReturnsAsync(propertyCategory);

            var result = await _propertyCategoryController.RenamePropertyCategory(propertyCategoryId, viewModel);
            _mockPropertyCategoryRepository.Verify(d => d.UpdatePropertyCategory(It.IsAny<PropertyCategory>()), Times.Never);
            _mockDocumentRepository.Verify(d => d.UpdateDocumentAsync(It.IsAny<Document>()), Times.Never);
            _mockBlobService.Verify(b => b.RenameBlobDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [TearDown]
        public void Teardown()
        {
            _propertyCategoryController.Dispose();
        }
    }
}

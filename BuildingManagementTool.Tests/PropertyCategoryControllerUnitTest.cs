using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class PropertyCategoryControllerUnitTest
    {
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private Mock<IPropertyRepository> _mockPropertyRepository;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private PropertyCategoryController _propertyCategoryController;

        [SetUp]
        public void Setup()
        {
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _propertyCategoryController = new PropertyCategoryController(_mockPropertyCategoryRepository.Object, _mockPropertyRepository.Object, _mockCategoryRepository.Object);
        }

        [Test]
        public async Task Index_PropertyExists_ReturnView()
        {
            int id = 1;
            Models.Property property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var list = new List<PropertyCategory>
            {
                new PropertyCategory{ PropertyCategoryId = 1 ,PropertyId = 1, CategoryId = 1 },
                new PropertyCategory{ PropertyCategoryId = 2 ,PropertyId = 1, CategoryId = 2 },
                new PropertyCategory{ PropertyCategoryId = 3 ,PropertyId = 2, CategoryId = 1 },
            };
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);
            _mockPropertyCategoryRepository.Setup(pc => pc.GetByPropertyId(id)).ReturnsAsync(list.Where(p => p.PropertyId == id));
            var img = "/imgs/sample-house.jpeg";
            var viewModel = new CategoryViewModel(list, img, property);

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
            Models.Property property = null;
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

            var result = await _propertyCategoryController.Index(id);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "Result should be of type ObjectResult.");
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound), "Expected 404 Not Found status code.");
        }

        [Test]
        public async Task UpdateCategoryCanvas_PropertyExists_ReturnPartial()
        {
            int id = 1;
            Models.Property property = new Models.Property { PropertyId = 1, PropertyName = "Test Property" };
            var list = new List<PropertyCategory>
            {
                new PropertyCategory{ PropertyCategoryId = 1 ,PropertyId = 1, CategoryId = 1 },
                new PropertyCategory{ PropertyCategoryId = 2 ,PropertyId = 1, CategoryId = 2 },
                new PropertyCategory{ PropertyCategoryId = 3 ,PropertyId = 2, CategoryId = 1 },
            };
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);
            _mockPropertyCategoryRepository.Setup(pc => pc.GetByPropertyId(id)).ReturnsAsync(list.Where(p => p.PropertyId == id));
            var img = "/imgs/sample-house.jpeg";
            var viewModel = new CategoryViewModel(list, img, property);

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
            Models.Property property = null;
            _mockPropertyRepository.Setup(p => p.GetById(id)).ReturnsAsync(property);

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
            var viewModel = new CategoryFormViewModel(list, id);

            var result = await _propertyCategoryController.CategoryFormPartial(id);
            var viewResult = result as PartialViewResult;
            var resultViewModel = viewResult.Model as CategoryFormViewModel;
            Assert.IsNotNull(result);
            Assert.That(viewResult.ViewName.Equals("_CategoryForm"));
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
            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(list);

            var result = await _propertyCategoryController.AddCategory(1, categoryName);
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
            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(list);

            var result = await _propertyCategoryController.AddCategory(1, categoryName);
            _mockPropertyCategoryRepository.Verify(repo => repo.AddPropertyCategory(It.IsAny<PropertyCategory>()), Times.Never);

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

using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class PropertyRepositoryUnitTest
    {
        private DbContextOptions<BuildingManagementToolDbContext> _options;
        private BuildingManagementToolDbContext _dbContext;
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private Mock<IUserPropertyRepository> _mockUserPropertyRepository;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private PropertyRepository _propertyRepository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<BuildingManagementToolDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
            _dbContext = new BuildingManagementToolDbContext(_options);
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _mockUserPropertyRepository = new Mock<IUserPropertyRepository>();
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _propertyRepository = new PropertyRepository(_dbContext, _mockCategoryRepository.Object, _mockUserPropertyRepository.Object, _mockDocumentRepository.Object, _mockPropertyCategoryRepository.Object);
        }

        [Test]
        public async Task AddProperty_ValidProperty_Success()
        {
            var property = new Property { PropertyId = 1, PropertyName = "Property1" };
            await _propertyRepository.AddProperty(property);
            var savedProperty = await _dbContext.Properties.FindAsync(1);
            Assert.IsNotNull(savedProperty);
            Assert.That(savedProperty.PropertyId, Is.EqualTo(1));
            Assert.That(savedProperty.PropertyName, Is.EqualTo("Property1"));
        }

        [Test]
        public async Task AddProperty_NullProperty_ThrowEx()
        {
            Property property = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyRepository.AddProperty(null));
        }

        [Test]
        public async Task AddDefaultCategories_ValidProperty()
        {
            var property = new Property { PropertyId = 1, PropertyName = "Property1" };
            var categoryList = new List<Category> 
            {
                new Category{CategoryId = 1, CategoryName = "Category1"},
                new Category{CategoryId = 2, CategoryName = "Category2"}
            };
            _mockCategoryRepository.Setup(c => c.Categories()).ReturnsAsync(categoryList);
            await _propertyRepository.AddDefaultCategories(property);
            var propertyCategories = await _dbContext.PropertyCategories.Where(pc => pc.PropertyId == property.PropertyId).ToListAsync();
            Assert.IsNotNull(propertyCategories);
            Assert.That(propertyCategories.Count, Is.EqualTo(2));
            Assert.That(propertyCategories[0].CategoryId, Is.EqualTo(1));
            Assert.That(propertyCategories[1].CategoryId, Is.EqualTo(2));
        }

        [Test]
        public async Task AddDefaultCategories_NullProperty_ThrowEx()
        {
            Property property = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyRepository.AddDefaultCategories(property));
        }

        [Test]
        public async Task DeleteProperty_ValidProperty_Success()
        {
            int id = 1;

            var propertyList = new List<Property>
            {
                new Property {PropertyId = 1, PropertyName = "Test Property"},
                new Property {PropertyId = 2, PropertyName = "Test Property 2" }
            };

            _dbContext.Properties.AddRange(propertyList);
            await _dbContext.SaveChangesAsync();

            var initialList = await _dbContext.Properties.ToListAsync();

            await _propertyRepository.DeleteProperty(propertyList[0]);

            var newList = await _dbContext.Properties.ToListAsync();
            Assert.That(newList.Count, Is.EqualTo(1));
            Assert.That(newList[0].PropertyId, Is.EqualTo(propertyList[1].PropertyId));
            Assert.That(newList[0].PropertyName, Is.EqualTo(propertyList[1].PropertyName));
        }

        [Test]
        public async Task DeleteProperty_NullProperty_ThrowEx()
        {
            Property property = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyRepository.DeleteProperty(property));
        }


        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }
    }
}

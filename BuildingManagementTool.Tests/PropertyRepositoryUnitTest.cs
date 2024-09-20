using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private PropertyRepository _propertyRepository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<BuildingManagementToolDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            _dbContext = new BuildingManagementToolDbContext(_options);

            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _propertyRepository = new PropertyRepository(_dbContext, _mockCategoryRepository.Object);
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
            await _propertyRepository.AddDefaultCategories(null));
        }


        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }
    }
}

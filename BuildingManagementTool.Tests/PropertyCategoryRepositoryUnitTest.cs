using BuildingManagementTool.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class PropertyCategoryRepositoryUnitTest
    {
        private DbContextOptions<BuildingManagementToolDbContext> _options;
        private BuildingManagementToolDbContext _dbContext;
        private PropertyCategoryRepository _propertyCategoryRepository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<BuildingManagementToolDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            _dbContext = new BuildingManagementToolDbContext(_options);
            _propertyCategoryRepository = new PropertyCategoryRepository(_dbContext);
        }

        [Test]
        public async Task AddPropertyCategory_PropertyCategoryExists_AddSuccess()
        {
            var newCategory = new PropertyCategory
            {
                PropertyCategoryId = 1,
                CategoryId = 1,
                PropertyId = 1
            };

            await _propertyCategoryRepository.AddPropertyCategory(newCategory);
            var savedDocument = await _dbContext.PropertyCategories.FindAsync(1);
            Assert.That(newCategory != null);
            Assert.That(newCategory.PropertyCategoryId.Equals(1));
            Assert.That(newCategory.CategoryId.Equals(1));
            Assert.That(newCategory.PropertyId.Equals(1));
        }

        [Test]
        public async Task AddPropertyCategory_PropertyCategoryNull_ThrowEx()
        {
            PropertyCategory newCategory = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyCategoryRepository.AddPropertyCategory(newCategory));
        }

        [Test]
        public async Task DeletePropertyCategory_PropertyCategoryExists_AddSuccess()
        {
            PropertyCategory newCategory = new PropertyCategory { PropertyCategoryId = 1, PropertyId = 1, CategoryId = 1 };
            await _propertyCategoryRepository.AddPropertyCategory(newCategory);
            await _propertyCategoryRepository.DeletePropertyCategory(newCategory);
            var savedCategory = await _dbContext.PropertyCategories.FindAsync(1);
            Assert.That(savedCategory == null);
        }

        [Test]
        public async Task DeletePropertyCategory_PropertyCategoryNull_ThrowEx()
        {
            PropertyCategory newCategory = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyCategoryRepository.AddPropertyCategory(newCategory));
        }

        [TearDown]
        public void Teardown()
        {
            _dbContext.Dispose();
        }
    }
}

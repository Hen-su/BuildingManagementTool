using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class UserPropertyRepositoryUnitTest
    {
        private DbContextOptions<BuildingManagementToolDbContext> _options;
        private BuildingManagementToolDbContext _dbContext;
        private UserPropertyRepository _userPropertyRepository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<BuildingManagementToolDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            _dbContext = new BuildingManagementToolDbContext(_options);
            _userPropertyRepository = new UserPropertyRepository(_dbContext);
        }

        [Test]
        public async Task GetByUserId_ValidId_ReturnPropertyList()
        {
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty> 
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0] },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1] }
            };
            await _dbContext.Properties.AddRangeAsync(propertyList);
            await _dbContext.UserProperties.AddRangeAsync(userPropertyList);
            await _dbContext.SaveChangesAsync();
            var returnedList = await _userPropertyRepository.GetByUserId(userId1);
            Assert.IsNotNull(returnedList);
            Assert.That(returnedList.Count, Is.EqualTo(1));
            Assert.That(returnedList.FirstOrDefault().PropertyId, Is.EqualTo(1));
            Assert.That(returnedList.FirstOrDefault().UserId, Is.EqualTo(userId1));
        }

        [Test]
        public async Task GetByUserId_InvalidId_ReturnEmpty()
        {
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var userId3 = Guid.NewGuid().ToString(); 
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty>
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0] },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1] }
            };
            await _dbContext.Properties.AddRangeAsync(propertyList);
            await _dbContext.UserProperties.AddRangeAsync(userPropertyList);
            await _dbContext.SaveChangesAsync();

            var returnedList = await _userPropertyRepository.GetByUserId(userId3);
            Assert.IsEmpty(returnedList);
        }

        [Test]
        public async Task GetByUserId_NullId_ThrowEx()
        { 
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _userPropertyRepository.GetByUserId(null));
        }

        [Test]
        public async Task AddUserProperty_ValidUserProperty_Success()
        {
            var userId = Guid.NewGuid().ToString();
            var userProperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId };

            await _userPropertyRepository.AddUserProperty(userProperty);
            var savedProperty = await _dbContext.UserProperties.FindAsync(1);
            Assert.IsNotNull(savedProperty);
            Assert.That(savedProperty.UserPropertyId, Is.EqualTo(1));
            Assert.That(savedProperty.PropertyId, Is.EqualTo(1));
            Assert.That(savedProperty.UserId, Is.EqualTo(userId));
        }

        [Test]
        public async Task AddUserProperty_NullUserProperty_ThrowEx()
        {
            UserProperty userProperty = null;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _userPropertyRepository.GetByUserId(null));
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }
    }
}

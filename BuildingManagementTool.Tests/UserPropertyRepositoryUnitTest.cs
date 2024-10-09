using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Identity;
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
            var roleId1 = Guid.NewGuid().ToString();
            var roleId2 = Guid.NewGuid().ToString();
            var managerRole = new IdentityRole { Id = roleId1, Name = "Manager" };
            var viewerRole = new IdentityRole { Id = roleId2, Name = "Viewer" };
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty>
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0], RoleId = roleId1, Role = managerRole },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1], RoleId = roleId2, Role = viewerRole }
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
            var roleId1 = Guid.NewGuid().ToString();
            var roleId2 = Guid.NewGuid().ToString();
            var managerRole = new IdentityRole { Id = roleId1, Name = "Manager" };
            var viewerRole = new IdentityRole { Id = roleId2, Name = "Viewer" };
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty>
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0], RoleId = roleId1, Role = managerRole },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1], RoleId = roleId2, Role = viewerRole }
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
            var roleId = Guid.NewGuid().ToString();
            var userProperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = roleId };

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

        [Test]
        public async Task DeleteByPropertyId_ValidId_Success()
        {
            int id = 1;
            var userId = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var userPropertyList = new List<UserProperty>
            {
                new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = roleId },
                new UserProperty {UserPropertyId = 2, PropertyId = 2, UserId = userId2, RoleId = roleId }
            };
            await _dbContext.UserProperties.AddRangeAsync(userPropertyList);
            await _dbContext.SaveChangesAsync();

            var initialList = await _dbContext.UserProperties.ToListAsync();

            await _userPropertyRepository.DeleteByPropertyId(id);

            var newList = await _dbContext.UserProperties.ToListAsync();
            Assert.That(newList.Count, Is.EqualTo(1));
            Assert.That(newList[0].UserId, Is.EqualTo(userId2));
        }

        [Test]
        public async Task DeleteByPropertyId_NullId_ThrowEx()
        {
            int id = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _userPropertyRepository.DeleteByPropertyId(id));
        }

        [Test]
        public async Task GetByPropertyId_ValidId_ReturnPropertyList()
        {
            var id = 1;
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty>
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0], RoleId = roleId },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1], RoleId = roleId }
            };
            await _dbContext.Properties.AddRangeAsync(propertyList);
            await _dbContext.UserProperties.AddRangeAsync(userPropertyList);
            await _dbContext.SaveChangesAsync();
            var returnedList = await _userPropertyRepository.GetByPropertyId(id);
            Assert.IsNotNull(returnedList);
            Assert.That(returnedList.Count, Is.EqualTo(1));
            Assert.That(returnedList.FirstOrDefault().PropertyId, Is.EqualTo(1));
            Assert.That(returnedList.FirstOrDefault().UserId, Is.EqualTo(userId1));
        }

        [Test]
        public async Task GetByPropertyId_NullId_ThrowEx()
        {
            int id = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _userPropertyRepository.GetByPropertyId(id));
        }

        [Test]
        public async Task DeleteByUserIdAndPropertyId_ValidId_ReturnPropertyList()
        {
            var id = 1;
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty>
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0], RoleId = roleId },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1], RoleId = roleId }
            };
            await _dbContext.Properties.AddRangeAsync(propertyList);
            await _dbContext.UserProperties.AddRangeAsync(userPropertyList);
            await _dbContext.SaveChangesAsync();
            await _userPropertyRepository.DeleteUserProperty(userPropertyList[0]);
            var deletedProperty = await _dbContext.UserProperties.FindAsync(1);
            Assert.IsNull(deletedProperty);
        }

        [Test]
        public async Task DeleteByUserIdAndPropertyId_NullId_ThrowEx()
        {
            var userproperty = (UserProperty) null;
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _userPropertyRepository.DeleteUserProperty(userproperty));
        }

        [Test]
        public async Task GetByPropertyIdAndUserId_ValidId_ReturnProperty()
        {
            var id = 1;
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var roleId1 = Guid.NewGuid().ToString();
            var roleId2 = Guid.NewGuid().ToString();
            var managerRole = new IdentityRole { Id = roleId1, Name = "Manager" };
            var viewerRole = new IdentityRole { Id = roleId2, Name = "Viewer" };
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty>
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0], RoleId = roleId1, Role = managerRole },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1], RoleId = roleId2, Role = viewerRole }
            };
            await _dbContext.Properties.AddRangeAsync(propertyList);
            await _dbContext.UserProperties.AddRangeAsync(userPropertyList);
            await _dbContext.SaveChangesAsync();
            var userproperty = await _userPropertyRepository.GetByPropertyIdAndUserId(2, userId2);
            Assert.IsNotNull(userproperty);
            Assert.That(userproperty.UserPropertyId, Is.EqualTo(2));
        }

        [Test]
        public async Task GetByPropertyIdAndUserId_InvalidId_ThrowEx()
        {
            int id = 0;
            var userId = Guid.NewGuid().ToString();
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _userPropertyRepository.GetByPropertyIdAndUserId(id, userId));
        }

        [Test]
        public async Task GetByPropertyIdAndUserId_InvalidUserId_ThrowEx()
        {
            int id = 1;
            var userId = "";
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _userPropertyRepository.GetByPropertyIdAndUserId(id, userId));
        }

        [Test]
        public async Task GetManagerUserIdByPropertyId_ValidId_ReturnUserProperty()
        {
            var id = 1;
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var roleId1 = Guid.NewGuid().ToString();
            var roleId2 = Guid.NewGuid().ToString();
            var managerRole = new IdentityRole { Id = roleId1, Name = "Manager" };
            var viewerRole = new IdentityRole { Id = roleId2, Name = "Viewer" };
            var propertyList = new List<Property>
            {
                new Property { PropertyId = 1, PropertyName = "Property 1" },
                new Property { PropertyId = 2, PropertyName = "Property 2" }
            };

            var userPropertyList = new List<UserProperty>
            {
                new UserProperty{ UserPropertyId = 1, PropertyId = 1, UserId = userId1, Property = propertyList[0], RoleId = roleId1, Role = managerRole },
                new UserProperty{ UserPropertyId = 2, PropertyId = 2, UserId = userId2, Property = propertyList[1], RoleId = roleId2, Role = viewerRole }
            };
            await _dbContext.Properties.AddRangeAsync(propertyList);
            await _dbContext.UserProperties.AddRangeAsync(userPropertyList);
            await _dbContext.SaveChangesAsync();
            var returnedID = await _userPropertyRepository.GetManagerUserIdByPropertyId(1);
            Assert.IsNotNull(returnedID);
            Assert.That(returnedID, Is.EqualTo(userId1));
        }

        [Test]
        public async Task GetManagerUserIdByPropertyId_InvalidId_ThrowEx()
        {
            var propertyId = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _userPropertyRepository.GetManagerUserIdByPropertyId(propertyId));
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }
    }
}

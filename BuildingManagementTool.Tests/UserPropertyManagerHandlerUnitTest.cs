using BuildingManagementTool.Models;
using BuildingManagementTool.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class UserPropertyManagerHandlerUnitTest
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<IUserPropertyRepository> _mockUserPropertyRepository;
        private UserPropertyManagerHandler _handler;

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            _mockUserPropertyRepository = new Mock<IUserPropertyRepository>();

            _handler = new UserPropertyManagerHandler(_mockUserManager.Object, _mockUserPropertyRepository.Object);
        }

        [Test]
        public async Task HandleRequirementAsync_UserIsManager_ShouldSucceed()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;

            var requirement = new UserPropertyManagerRequirement(propertyId);
            var userProperty = new UserProperty
            {
                UserId = userId,
                PropertyId = propertyId,
                Role = new IdentityRole { Name = "Manager" }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _mockUserPropertyRepository.Setup(repo => repo.GetByPropertyIdAndUserId(propertyId, userId))
                .ReturnsAsync(userProperty);

            var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);

            await _handler.HandleAsync(context);

            Assert.IsTrue(context.HasSucceeded);
        }

        [Test]
        public async Task HandleRequirementAsync_UserIsNotManager_ShouldFail()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;

            var requirement = new UserPropertyManagerRequirement(propertyId);
            var userProperty = new UserProperty
            {
                UserId = userId,
                PropertyId = propertyId,
                Role = new IdentityRole { Name = "Viewer" }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _mockUserPropertyRepository.Setup(repo => repo.GetByPropertyIdAndUserId(propertyId, userId))
                .ReturnsAsync(userProperty);

            var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);

            await _handler.HandleAsync(context);

            Assert.IsFalse(context.HasSucceeded);
        }

        [Test]
        public async Task HandleRequirementAsync_UserNotFound_ShouldFail()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;

            var requirement = new UserPropertyManagerRequirement(propertyId);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(string.Empty);

            var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);

            await _handler.HandleAsync(context);

            Assert.IsFalse(context.HasSucceeded);
        }

        [Test]
        public async Task HandleRequirementAsync_UserPropertyNotFound_ShouldFail()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;

            var requirement = new UserPropertyManagerRequirement(propertyId);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            _mockUserPropertyRepository.Setup(repo => repo.GetByPropertyIdAndUserId(propertyId, userId))
                .ReturnsAsync((UserProperty)null);

            var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);

            await _handler.HandleAsync(context);

            Assert.IsFalse(context.HasSucceeded);
        }
    }
}


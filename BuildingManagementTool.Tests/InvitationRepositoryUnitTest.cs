using BuildingManagementTool.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class InvitationRepositoryUnitTest
    {
        private DbContextOptions<BuildingManagementToolDbContext> _options;
        private BuildingManagementToolDbContext _dbContext;
        private InvitationRepository _invitationRepository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<BuildingManagementToolDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            _dbContext = new BuildingManagementToolDbContext(_options);
            _invitationRepository = new InvitationRepository(_dbContext);
        }

        [Test]
        public async Task GetInvitationByEmailAndPropertyAsync()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;
            var email1 = "example@example.com";
            var email2 = "example2@example.com";
            var invitationList = new List<Invitation>
            {
                new Invitation{ InvitationId = 1, Email = email1, SentOn = DateTime.UtcNow, PropertyId = propertyId, Status = "Pending", InvitedBy = userId},
                new Invitation{ InvitationId = 2, Email = email2, SentOn = DateTime.UtcNow, PropertyId = propertyId, Status = "Pending", InvitedBy = userId}
            };
            _dbContext.Invitations.AddRange(invitationList);
            await _dbContext.SaveChangesAsync();

            var invitation = await _invitationRepository.GetInvitationByEmailAndPropertyAsync(email1, propertyId);
            Assert.IsNotNull(invitation);
            Assert.That(invitation.InvitationId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetPendingInvitationsByEmailAsync()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;
            var propertyId2 = 2;
            var email = "example@example.com";
            var invitationList = new List<Invitation>
            {
                new Invitation{ InvitationId = 1, Email = email, SentOn = DateTime.UtcNow, PropertyId = propertyId, Status = "Pending", InvitedBy = userId},
                new Invitation{ InvitationId = 2, Email = email, SentOn = DateTime.UtcNow, PropertyId = propertyId, Status = "Completed", InvitedBy = userId}
            };
            _dbContext.Invitations.AddRange(invitationList);
            await _dbContext.SaveChangesAsync();

            var invitation = await _invitationRepository.GetPendingInvitationsByEmailAsync(email);
            Assert.IsNotNull(invitation);
            Assert.That(invitation.Count, Is.EqualTo(1));
            Assert.That(invitation[0].InvitationId, Is.EqualTo(1));
            Assert.That(invitation[0].Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task AddInvitationAsync()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;
            var email = "example@example.com";
            var newInvitation = new Invitation { InvitationId = 1, Email = email, SentOn = DateTime.UtcNow, PropertyId = propertyId, Status = "Pending", InvitedBy = userId };

            await _invitationRepository.AddInvitationAsync(newInvitation);
            var invitation = _dbContext.Invitations.FindAsync(1);
            Assert.IsNotNull(invitation);
        }

        [Test]
        public async Task DeleteInvitationAsync()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;
            var email = "example@example.com";
            var newInvitation = new Invitation { InvitationId = 1, Email = email, SentOn = DateTime.UtcNow, PropertyId = propertyId, Status = "Pending", InvitedBy = userId };

            _dbContext.Invitations.Add(newInvitation);
            await _dbContext.SaveChangesAsync();

            await _invitationRepository.DeleteInvitationAsync(newInvitation.InvitationId);
            var invitation = await _dbContext.Invitations.FindAsync(newInvitation.InvitationId);
            Assert.IsNull(invitation);
        }

        [Test]
        public async Task UpdateInvitationAsync()
        {
            var userId = Guid.NewGuid().ToString();
            var propertyId = 1;
            var email = "example@example.com";
            var newInvitation = new Invitation { InvitationId = 1, Email = email, SentOn = DateTime.UtcNow, PropertyId = propertyId, Status = "Pending", InvitedBy = userId };

            _dbContext.Invitations.Add(newInvitation);
            await _dbContext.SaveChangesAsync();

            newInvitation.Status = "Completed";
            newInvitation.AcceptedOn = DateTime.UtcNow; 

            await _invitationRepository.UpdateInvitationAsync(newInvitation);
            var invitation = await _dbContext.Invitations.FindAsync(newInvitation.InvitationId);
            Assert.IsNotNull(invitation);
            Assert.That(invitation.Status, Is.EqualTo("Completed"));
            Assert.IsNotNull(invitation.AcceptedOn);
        }

        [TearDown]
        public void Teardown()
        {
            _dbContext.Dispose();
        }
    }
}

using Azure.Core;
using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc.Abstractions;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BuildingManagementTool.Services
{
    public class InvitationService
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;
        private readonly IInvitationRepository _invitationRepository;

        public InvitationService(BuildingManagementToolDbContext dbContext, UserManager<ApplicationUser> userManager, IEmailSender emailSender, 
            RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, IUrlHelperFactory urlHelperFactory, IRazorViewToStringRenderer razorViewToStringRenderer, IInvitationRepository invitationRepository)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _invitationRepository = invitationRepository;
        }

        public async Task InviteUserAsync(string email, int propertyId)
        {
            
            var existingInvitations = await _invitationRepository.GetInvitationByEmailAndPropertyAsync(email, propertyId);
            if (existingInvitations != null) 
            {
                return;
            }
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                {
                    // If the user exists, link them to the property
                    await LinkExistingUserToPropertyAsync(existingUser, propertyId);
                }
                else
                {
                    // If the user doesn't exist, send an email invite (without a token)
                    await SendInvitationEmailAsync(email, propertyId);
                }
        }

        private async Task LinkExistingUserToPropertyAsync(ApplicationUser user, int propertyId)
        {
            var existingLink = await _dbContext.UserProperties
                .FirstOrDefaultAsync(up => up.UserId == user.Id && up.PropertyId == propertyId);

            if (existingLink == null)
            {
                var role = await _roleManager.FindByNameAsync("Viewer");
                var userProperty = new UserProperty
                {
                    UserId = user.Id,
                    PropertyId = propertyId,
                    RoleId = role.Id
                };

                _dbContext.UserProperties.Add(userProperty);
                await _dbContext.SaveChangesAsync();
                //set up email content
                var httpContext = _httpContextAccessor.HttpContext;
                var urlHelper = _urlHelperFactory.GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));
                var callbackUrl = urlHelper.Page(
                        "/Account/Login",
                        pageHandler: null,
                        values: new { area = "Identity" },
                        protocol: httpContext.Request.Scheme);

                var model = new EmailViewModel { Username = user.FirstName, EmailLink = callbackUrl };
                var viewPath = "Shared/EmailTemplates/InviteToLogin";
                var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync(viewPath, model, httpContext);
                // notify the user via email
                await _emailSender.SendEmailAsync(user.Email, "New Property Access",
                    htmlContent);
            }
        }

        private async Task SendInvitationEmailAsync(string email, int propertyId)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var invitation = new Invitation
            {
                Email = email,
                PropertyId = propertyId,
                Status = "Pending",
                SentOn = DateTime.UtcNow,
                InvitedBy = user.Id
            };

            _dbContext.Invitations.Add(invitation);
            await _dbContext.SaveChangesAsync();
            //set up email content
            var httpContext = _httpContextAccessor.HttpContext;
            var urlHelper = _urlHelperFactory.GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));
            var callbackUrl = urlHelper.Page(
                    "/Account/Register",
                    pageHandler: null,
                    values: new { area = "Identity" },
                    protocol: httpContext.Request.Scheme);
            
            var model = new EmailViewModel { Username = user.FirstName, EmailLink = callbackUrl };
            var viewPath = "Shared/EmailTemplates/InviteToRegister";
            var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync(viewPath, model, httpContext);

            //send invite to register
            await _emailSender.SendEmailAsync(email, "Invite to view property",
                htmlContent);
        }

        public async Task LinkUserToPropertyOnRegisterConfirm(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var invitations = await _invitationRepository.GetPendingInvitationsByEmailAsync(email);
            if (invitations != null)
            {
                foreach (var invite in invitations)
                {
                    var role = await _roleManager.FindByNameAsync("Viewer");
                    var userProperty = new UserProperty
                    {
                        UserId = user.Id,
                        PropertyId = invite.PropertyId,
                        RoleId = role.Id
                    };

                    invite.Status = "Completed";
                    invite.AcceptedOn = DateTime.UtcNow;
                    _dbContext.Invitations.Update(invite);
                    _dbContext.UserProperties.Add(userProperty);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
  
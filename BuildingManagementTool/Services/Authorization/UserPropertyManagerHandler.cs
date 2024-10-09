using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Services.Authorization
{
    public class UserPropertyManagerHandler : AuthorizationHandler<UserPropertyManagerRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserPropertyRepository _userPropertyRepository;

        public UserPropertyManagerHandler(UserManager<ApplicationUser> userManager, IUserPropertyRepository userPropertyRepository)
        {
            _userManager = userManager;
            _userPropertyRepository = userPropertyRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserPropertyManagerRequirement requirement)
        {
            var userId = _userManager.GetUserId(context.User);

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail(); 
                return;
            }

            // Query the UserProperty table to find the user's role for the given property
            var userProperty = await _userPropertyRepository.GetByPropertyIdAndUserId(requirement.PropertyId, userId);

            // Ensure we have a UserProperty and check if the user is a "Manager"
            if (userProperty != null && userProperty.Role.Name == "Manager")
            {
                context.Succeed(requirement);  // Authorization succeeds if the user is a Manager for the property
            }
            else
            {
                context.Fail();  // Fail if the user is not a Manager for the property
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;

namespace BuildingManagementTool.Services.Authorization
{
    public class UserPropertyManagerRequirement : IAuthorizationRequirement
    {
        public int PropertyId { get; }

        public UserPropertyManagerRequirement(int propertyId)
        {
            PropertyId = propertyId;
        }
    }
}

namespace BuildingManagementTool.Models
{
    public interface IInvitationService
    {
        Task InviteUserAsync(string email, int propertyId);
        Task LinkUserToPropertyOnRegisterConfirm(string email);
    }
}

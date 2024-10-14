namespace BuildingManagementTool.Models
{
    public interface IInvitationRepository
    {
        Task<Invitation> GetInvitationByEmailAndPropertyAsync(string email, int propertyId);
        Task<List<Invitation>> GetPendingInvitationsByEmailAsync(string email);
        Task AddInvitationAsync(Invitation invitation);
        Task DeleteInvitationAsync(int invitationId);
        Task UpdateInvitationAsync(Invitation invitation);
    }
}

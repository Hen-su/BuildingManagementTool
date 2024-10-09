using Microsoft.EntityFrameworkCore;
namespace BuildingManagementTool.Models
    
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        public InvitationRepository(BuildingManagementToolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Invitation> GetInvitationByEmailAndPropertyAsync(string email, int propertyId)
        {
            return await _dbContext.Invitations
                .FirstOrDefaultAsync(i => i.Email == email && i.PropertyId == propertyId && i.Status == "Pending");
        }

        public async Task<List<Invitation>> GetPendingInvitationsByEmailAsync(string email)
        {
            return await _dbContext.Invitations
                .Where(i => i.Email == email && i.Status == "Pending")
                .ToListAsync();
        }

        public async Task AddInvitationAsync(Invitation invitation)
        {
            _dbContext.Invitations.Add(invitation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteInvitationAsync(int invitationId)
        {
            var invitation = await _dbContext.Invitations.FindAsync(invitationId);
            if (invitation != null)
            {
                _dbContext.Invitations.Remove(invitation);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateInvitationAsync(Invitation invitation)
        {
            _dbContext.Invitations.Update(invitation);
            await _dbContext.SaveChangesAsync();
        }
    }
}

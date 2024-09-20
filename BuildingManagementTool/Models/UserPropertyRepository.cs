
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class UserPropertyRepository : IUserPropertyRepository
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        public UserPropertyRepository(BuildingManagementToolDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<UserProperty>> GetByUserId(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "UserId cannot be null.");
            }
            return await _dbContext.UserProperties.Where(u => u.UserId == userId).Include(u => u.Property).ToListAsync();
        }

        public async Task AddUserProperty(UserProperty property)
        {
            await _dbContext.UserProperties.AddAsync(property);
            await _dbContext.SaveChangesAsync();
        }
    }
}

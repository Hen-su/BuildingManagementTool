
using Microsoft.AspNetCore.Http.HttpResults;
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

        public async Task DeleteByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property id cannot be null.");
            }
            var userPropertyList = await _dbContext.UserProperties.Where(u => u.PropertyId == id).ToListAsync();
            _dbContext.UserProperties.RemoveRange(userPropertyList);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserProperty>> GetByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property id cannot be null.");
            }
            var userPropertyList = await _dbContext.UserProperties.Where(u => u.PropertyId==id).ToListAsync();
            return userPropertyList;
        }

        public async Task DeleteByUserIdAndPropertyId(int id, string userId)
        {
            if (id == 0)
            {
                throw new ArgumentNullException("Property id cannot be null.");
            }
            if (userId == null || userId == "")
            {
                throw new ArgumentNullException("Email cannot be null.");
            }
            var userProperty = await _dbContext.UserProperties.FirstOrDefaultAsync(u => u.PropertyId == id && u.UserId == userId);
            if (userProperty == null) 
            {
                throw new InvalidOperationException($"No user-property association found for Property ID {id} and User ID {userId}.");
            }
            _dbContext.UserProperties.Remove(userProperty);
            await _dbContext.SaveChangesAsync();
        }
    }
}

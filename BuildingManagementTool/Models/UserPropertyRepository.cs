
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
            return await _dbContext.UserProperties.Where(u => u.UserId == userId).Include(u => u.Property).Include(u => u.Role).ToListAsync();
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

        public async Task<UserProperty> GetByPropertyIdAndUserId(int id, string userId)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property id cannot be null.");
            }
            if (userId == null || userId == "")
            {
                throw new ArgumentNullException("UserId cannot be null.");
            }
            var userProperty = await _dbContext.UserProperties.Include(u => u.Property).Include(u => u.Role).FirstOrDefaultAsync(u => u.PropertyId == id && u.UserId == userId);
            return userProperty;
        }

        public async Task DeleteUserProperty(UserProperty userProperty)
        {
            if (userProperty == null) 
            {
                throw new InvalidOperationException("Invalid user property");
            }
            _dbContext.UserProperties.Remove(userProperty);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> GetManagerUserIdByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property id cannot be null.");
            }
            var userproperty = await _dbContext.UserProperties.FirstOrDefaultAsync(p => p.Role.Name == "Manager");
            return userproperty.UserId;
        }
    }
}

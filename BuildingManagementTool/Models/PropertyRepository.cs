
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        public PropertyRepository(BuildingManagementToolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Property>> Properties()
        {
            return await _dbContext.Properties.ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetByUserId(string id) => throw new NotImplementedException();
        //return await _dbContext.Properties.Where(u => u.UserId).ToListAsync();
    }
}

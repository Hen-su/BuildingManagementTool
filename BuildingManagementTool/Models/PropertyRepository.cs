
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

        public async Task<Property> GetById(int id)
        {
            if (id != null)
            {
                return await _dbContext.Properties.FirstOrDefaultAsync(p => p.PropertyId == id);
            }
            return null;
        }


    }
}

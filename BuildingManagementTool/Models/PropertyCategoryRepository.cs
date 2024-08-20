
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class PropertyCategoryRepository : IPropertyCategoryRepository
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        public PropertyCategoryRepository(BuildingManagementToolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<PropertyCategory>> PropertyCategories()
        { 
            return await _dbContext.PropertyCategories.ToListAsync();
        }


        public async Task<IEnumerable<PropertyCategory>> GetByPropertyId(int? id)
        {
            return await _dbContext.PropertyCategories.Where(p => p.PropertyId == id).Include(pc => pc.Category).ToListAsync();
        }
    }
}

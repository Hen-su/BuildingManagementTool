
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

        public async Task<PropertyCategory> GetById(int id)
        {
            return await _dbContext.PropertyCategories.Include(pc => pc.Category).Include(pc => pc.Property).FirstOrDefaultAsync(pc => pc.PropertyCategoryId == id);
        }

        public async Task<IEnumerable<PropertyCategory>> GetByPropertyId(int? id)
        {
            return await _dbContext.PropertyCategories.Where(p => p.PropertyId == id).Include(pc => pc.Category).ToListAsync();
        }

        public async Task AddPropertyCategory(PropertyCategory propertyCategory)
        {
            if (propertyCategory == null)
            {
                throw new ArgumentNullException(nameof(propertyCategory), "PropertyCategory cannot be null.");
            }
            await _dbContext.PropertyCategories.AddAsync(propertyCategory);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePropertyCategory(PropertyCategory propertyCategory)
        {
            if (propertyCategory == null)
            {
                throw new ArgumentNullException(nameof(propertyCategory), "PropertyCategory cannot be null.");
            }
              _dbContext.PropertyCategories.Remove(propertyCategory);
             await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePropertyCategory(PropertyCategory propertyCategory)
        {
            if (propertyCategory == null)
            {
                throw new ArgumentNullException(nameof(propertyCategory), "PropertyCategory cannot be null.");
            }
            _dbContext.PropertyCategories.Update(propertyCategory);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property Id cannot be null.");
            }
            var propertyCategoryList = await _dbContext.PropertyCategories.Where(pc => pc.PropertyId == id).ToListAsync();
            _dbContext.PropertyCategories.RemoveRange(propertyCategoryList);
            await _dbContext.SaveChangesAsync();
        }
    }
}

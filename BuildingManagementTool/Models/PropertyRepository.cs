
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        private readonly ICategoryRepository _categoryRepository;
        public PropertyRepository(BuildingManagementToolDbContext dbContext, ICategoryRepository categoryRepository)
        {
            _dbContext = dbContext;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Property>> Properties()
        {
            return await _dbContext.Properties.ToListAsync();
        }

        public async Task<Property> GetById(int id)
        {
            if (id != null)
            {
                return await _dbContext.Properties.FirstOrDefaultAsync(p => p.PropertyId == id);
            }
            return null;
        }

        public async Task AddProperty(Property property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property), "Property cannot be null.");
            }
            await _dbContext.Properties.AddAsync(property);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AddDefaultCategories(Property property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property), "Property cannot be null.");
            }
            var categories = await _categoryRepository.Categories();
            var defaultList = new List<PropertyCategory>();
            foreach (var category in categories)
            {
                defaultList.Add(new PropertyCategory
                {
                    CategoryId = category.CategoryId,
                    PropertyId = property.PropertyId
                });
            }
            await _dbContext.PropertyCategories.AddRangeAsync(defaultList);
            await _dbContext.SaveChangesAsync();
        }
    }
}

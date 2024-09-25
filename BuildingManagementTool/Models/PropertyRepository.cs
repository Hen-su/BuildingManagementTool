
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserPropertyRepository _userPropertyRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        public PropertyRepository(BuildingManagementToolDbContext dbContext, ICategoryRepository categoryRepository, IUserPropertyRepository userPropertyRepository, IDocumentRepository documentRepository, IPropertyCategoryRepository propertyCategoryRepository)
        {
            _dbContext = dbContext;
            _categoryRepository = categoryRepository;
            _userPropertyRepository = userPropertyRepository;
            _documentRepository = documentRepository;
            _propertyCategoryRepository = propertyCategoryRepository;
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

        public async Task DeleteProperty(Property property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("Property id cannot be null.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _documentRepository.DeleteByPropertyId(property.PropertyId);
                await _propertyCategoryRepository.DeleteByPropertyId(property.PropertyId);
                await _userPropertyRepository.DeleteByPropertyId(property.PropertyId);

                _dbContext.Properties.Remove(property);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }

        public async Task UpdateProperty(Property property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("Property id cannot be null.");
            }
            _dbContext.Properties.Update(property);
            await _dbContext.SaveChangesAsync();
        }
    }
}

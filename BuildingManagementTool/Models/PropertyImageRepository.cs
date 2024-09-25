
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class PropertyImageRepository : IPropertyImageRepository
    {
        private readonly BuildingManagementToolDbContext _dbContext;
        public PropertyImageRepository(BuildingManagementToolDbContext dbContext) 
        {
            _dbContext = dbContext; 
        }

        public async Task AddPropertyImage(PropertyImage propertyImage)
        {
            if (propertyImage == null)
            {
                throw new ArgumentNullException(nameof(propertyImage), "PropertyImage cannot be null.");
            }
            await _dbContext.PropertyImages.AddAsync(propertyImage);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("PropertyId cannot be null.");
            }
            var imageList = _dbContext.PropertyImages.Where(pi => pi.PropertyId == id).ToList();
            _dbContext.PropertyImages.RemoveRange(imageList);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePropertyImage(PropertyImage propertyImage)
        {
            if (propertyImage == null)
            {
                throw new ArgumentNullException(nameof(propertyImage), "PropertyImage cannot be null.");
            }
            _dbContext.PropertyImages.Remove(propertyImage);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PropertyImage> GetById(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property Image Id cannot be null.");
            }
            return _dbContext.PropertyImages.FirstOrDefault(pi => pi.PropertyId == id);
        }

        public async Task<IEnumerable<PropertyImage>> GetByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("PropertyId cannot be null.");
            }
            return _dbContext.PropertyImages.Where(pi => pi.PropertyId == id).ToList();
        }

        public IEnumerable<PropertyImage> PropertyImages
        {
            get { return _dbContext.PropertyImages; }
        }
    }
}

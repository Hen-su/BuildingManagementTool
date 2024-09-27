
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
        
        public async Task<PropertyImage> GetByFileName(int id, string fileName)
        {
            if (fileName == null || fileName == "")
            {
                throw new ArgumentNullException("File name cannot be null.");
            }
            return _dbContext.PropertyImages.FirstOrDefault(pi => pi.PropertyId == id && pi.FileName == fileName);
        }

        public async Task SetDisplayImage(PropertyImage propertyImage)
        {
            if (propertyImage == null)
            {
                throw new ArgumentNullException(nameof(propertyImage), "PropertyImage cannot be null.");
            }

            var propertyImageList = await GetByPropertyId(propertyImage.PropertyId);
            if (propertyImageList == null)
            {
                throw new ArgumentException("No images found for the given property ID.");
            }
            foreach (var image in propertyImageList)
            {
                image.IsDisplay = false;
            }
            propertyImage.IsDisplay = true;
            _dbContext.UpdateRange(propertyImageList);
            _dbContext.Update(propertyImage);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveDisplayImage(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("PropertyId cannot be null.");
            }
            var propertyImageList = await GetByPropertyId(id);
            if (propertyImageList == null)
            {
                throw new ArgumentException("No images found for the given property ID.");
            }
            foreach (var image in propertyImageList)
            {
                image.IsDisplay = false;
            }
            _dbContext.UpdateRange(propertyImageList);
            await _dbContext.SaveChangesAsync();
        }
    }
}

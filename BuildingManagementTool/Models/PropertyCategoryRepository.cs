﻿
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
            return await _dbContext.PropertyCategories.Include(pc => pc.Category).FirstOrDefaultAsync(pc => pc.PropertyCategoryId == id);
        }

        public async Task<IEnumerable<PropertyCategory>> GetByPropertyId(int? id)
        {
            return await _dbContext.PropertyCategories.Where(p => p.PropertyId == id).Include(pc => pc.Category).ToListAsync();
        }

        public async Task AddPropertyCategory(PropertyCategory propertyCategory)
        {
            if (propertyCategory != null) 
            { 
                await _dbContext.PropertyCategories.AddAsync(propertyCategory);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

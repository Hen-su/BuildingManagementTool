
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly BuildingManagementToolDbContext _buildingManagementToolDbContext;
        public CategoryRepository(BuildingManagementToolDbContext context)
        {
            _buildingManagementToolDbContext = context;
        }

        public async Task<IEnumerable<Category>> Categories()
        {
            return await _buildingManagementToolDbContext.Categories.ToListAsync();
        }

        public Task DeleteCategory(Category category)
        {
            throw new NotImplementedException();
        }

        public Task RenameCategory(Category category)
        {
            throw new NotImplementedException();
        }

        public List<Category> ListDefault() 
        {
            List<Category> list = new List<Category>
            {
                new Category { CategoryName = "Plumbing/Electrical" },
                new Category { CategoryName = "Design Engineering" },
                new Category { CategoryName = "Head Contractor" },
                new Category { CategoryName = "Consents" },
                new Category { CategoryName = "Flooring" },
                new Category { CategoryName = "Site Clearance" },
                new Category { CategoryName = "Painting Tiles" },
                new Category { CategoryName = "Foundation" },
                new Category { CategoryName = "Kitchen/Bathroom" },
                new Category { CategoryName = "Roof" },
                new Category { CategoryName = "Door/Windows" },
                new Category { CategoryName = "Framing/Carpenter" }
            };
            return list; 
        }
    }
}

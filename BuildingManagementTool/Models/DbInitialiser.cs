using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class DbInitialiser
    {
        public async static void Seed(IApplicationBuilder applicationBuilder)
        {
            BuildingManagementToolDbContext context =
                applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetService<BuildingManagementToolDbContext>();

            context.Database.Migrate();

            if (!context.Categories.Any())
            {
                context.Categories.AddRange
                (
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
                );
                context.SaveChanges();
            }

            if (!context.Properties.Any())
            {
                context.Properties.Add
                (
                    new Property { PropertyName = "Test Property",  }
                );
                context.SaveChanges();
            }

            if (!context.PropertyCategories.Any())
            {
                context.PropertyCategories.AddRange
                (
                    new PropertyCategory { PropertyId = 1, CategoryId = 1 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 2 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 3 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 4 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 5 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 6 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 7 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 8 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 9 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 10 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 11 },
                    new PropertyCategory { PropertyId = 1, CategoryId = 12 }
                );
                context.SaveChanges();
            }
        }
    }
}

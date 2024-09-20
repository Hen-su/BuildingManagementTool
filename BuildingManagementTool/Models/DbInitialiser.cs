using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingManagementTool.Models
{
    public static class DbInitializer
    {
        public async static void Seed(IApplicationBuilder applicationBuilder)
        {
            using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<BuildingManagementToolDbContext>();
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.Migrate();

            string[] roleNames = { "Viewer", "Manager" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
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
                context.Properties.Add(new Property { PropertyName = "Test Property" });
                context.SaveChanges();
            }

            if (!context.PropertyCategories.Any())
            {
                context.PropertyCategories.AddRange(
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

using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class BuildingManagementToolDbContext : DbContext
    {
        public BuildingManagementToolDbContext(DbContextOptions<BuildingManagementToolDbContext> options) : base(options) { }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PropertyCategory> PropertyCategories { get; set; }
        public DbSet<Property> Properties { get; set; }
    }
}

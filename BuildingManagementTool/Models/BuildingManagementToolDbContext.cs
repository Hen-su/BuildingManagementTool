using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class BuildingManagementToolDbContext : IdentityDbContext<ApplicationUser>
    {
        public BuildingManagementToolDbContext(DbContextOptions<BuildingManagementToolDbContext> options) : base(options) { }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PropertyCategory> PropertyCategories { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<UserProperty> UserProperties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
    }
}

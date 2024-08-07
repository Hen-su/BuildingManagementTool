using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class BuildingManagementToolDbContext : DbContext
    {
        public BuildingManagementToolDbContext(DbContextOptions<BuildingManagementToolDbContext> options) : base(options) { }
        public DbSet<File> Files { get; set; }
    }
}

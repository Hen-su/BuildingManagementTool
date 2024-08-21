using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class BuildingManagementToolDbContext : IdentityDbContext
    {
        public BuildingManagementToolDbContext(DbContextOptions<BuildingManagementToolDbContext> options) : base(options) { }
        public DbSet<Document> Documents { get; set; }
    }
}

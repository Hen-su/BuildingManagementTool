using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class BuildingManagementToolDbContext : IdentityDbContext
    {
        public BuildingManagementToolDbContext(DbContextOptions<BuildingManagementToolDbContext> options) : base(options) { }
        public DbSet<Document> Documents { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;

namespace BuildingManagementTool.Models
{
    public class BuildingManagementToolDbContext : DbContext
    {
        DbSet<File> Files { get; set; }
    }
}

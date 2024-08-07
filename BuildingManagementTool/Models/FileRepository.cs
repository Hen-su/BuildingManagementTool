
namespace BuildingManagementTool.Models
{
    public class FileRepository : IFileRepository
    {
        private readonly BuildingManagementToolDbContext _buildingManagementToolDbContext;
        public FileRepository(BuildingManagementToolDbContext buildingManagementToolDbContext)
        {
            _buildingManagementToolDbContext = buildingManagementToolDbContext;
        }

        public IEnumerable<File> AllFiles
        {
            get
            {
                return _buildingManagementToolDbContext.Files;
            }
        }

        public async Task AddFileData(File file)
        {
            await _buildingManagementToolDbContext.AddAsync(file);
            await _buildingManagementToolDbContext.SaveChangesAsync();
        }

        public IEnumerable<File> GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}

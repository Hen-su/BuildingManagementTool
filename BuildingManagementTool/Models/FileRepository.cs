
namespace BuildingManagementTool.Models
{
    public class FileRepository : IFileRepository
    {
        private readonly BuildingManagementToolDbContext _buildingManagementToolDbContext;
        public FileRepository(BuildingManagementToolDbContext buildingManagementToolDbContext)
        {
            _buildingManagementToolDbContext = buildingManagementToolDbContext;
        }

        public IEnumerable<File> AllFiles => throw new NotImplementedException();

        public void AddFileData(File file)
        {
            _buildingManagementToolDbContext.Add(file);
        }

        public IEnumerable<File> GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}

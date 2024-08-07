
namespace BuildingManagementTool.Models
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly BuildingManagementToolDbContext _buildingManagementToolDbContext;
        public DocumentRepository(BuildingManagementToolDbContext buildingManagementToolDbContext)
        {
            _buildingManagementToolDbContext = buildingManagementToolDbContext;
        }

        public IEnumerable<Document> AllDocuments
        {
            get
            {
                return _buildingManagementToolDbContext.Documents;
            }
        }

        public async Task AddDocumentData(Document file)
        {
            await _buildingManagementToolDbContext.AddAsync(file);
            await _buildingManagementToolDbContext.SaveChangesAsync();
        }

        public IEnumerable<Document> GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}

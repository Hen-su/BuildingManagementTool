
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;

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

        public async Task<Document> GetById(int? id)
        {
             return await _buildingManagementToolDbContext.Documents.FirstOrDefaultAsync(d => d.DocumentId == id);
        }

        public async Task<bool> DeleteDocumentData(Document document)
        {
            if (document != null) 
            {
                try
                {
                    _buildingManagementToolDbContext.Remove(document);
                    await _buildingManagementToolDbContext.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public async Task UpdateDocumentAsync(Document document)
        {
            _buildingManagementToolDbContext.Documents.Update(document);
            await _buildingManagementToolDbContext.SaveChangesAsync();
        }
    }
}

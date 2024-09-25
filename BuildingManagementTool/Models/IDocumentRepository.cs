namespace BuildingManagementTool.Models
{
    public interface IDocumentRepository
    {
        IEnumerable<Document> AllDocuments { get; }
        Task<Document> GetById (int id);
        Task AddDocumentData(Document document);
        Task<bool> DeleteDocumentData(Document document);
        Task<List<Document>> GetByPropertyCategoryId(int id);
        Task UpdateDocumentAsync(Document document);
        Task DeleteByPropertyId(int id);
        Task<List<Document>> GetByPropertyId(int id);
        Task UpdateByList(List<Document> documents);
    }
}

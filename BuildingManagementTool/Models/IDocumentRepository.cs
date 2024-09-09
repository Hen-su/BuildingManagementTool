namespace BuildingManagementTool.Models
{
    public interface IDocumentRepository
    {
        IEnumerable<Document> AllDocuments { get; }
        Task<Document> GetById (int? id);
        Task AddDocumentData(Document document);
        Task<bool> DeleteDocumentData(Document document);
        Task UpdateDocumentAsync(Document document);

    }
}

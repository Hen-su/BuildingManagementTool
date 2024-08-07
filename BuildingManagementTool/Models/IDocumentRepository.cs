namespace BuildingManagementTool.Models
{
    public interface IDocumentRepository
    {
        IEnumerable<Document> AllDocuments { get; }
        IEnumerable<Document> GetById (int id);
        Task AddDocumentData(Document file);
    }
}

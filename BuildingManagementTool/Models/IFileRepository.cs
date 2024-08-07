namespace BuildingManagementTool.Models
{
    public interface IFileRepository
    {
        IEnumerable<File> AllFiles { get; }
        IEnumerable<File> GetById (int id);
        Task AddFileData(File file);
    }
}

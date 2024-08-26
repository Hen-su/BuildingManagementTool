namespace BuildingManagementTool.Models
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> Categories();
        Task AddDefault();
        Task DeleteCategory(Category category);
        Task RenameCategory(Category category);
    }
}

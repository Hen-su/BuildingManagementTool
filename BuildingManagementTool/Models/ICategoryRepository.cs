namespace BuildingManagementTool.Models
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> Categories();
        Task DeleteCategory(Category category);
        Task RenameCategory(Category category);
    }
}

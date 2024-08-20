namespace BuildingManagementTool.Models
{
    public interface IPropertyCategoryRepository
    {
        Task<IEnumerable<PropertyCategory>> PropertyCategories();
        Task<IEnumerable<PropertyCategory>> GetByPropertyId(int? id);
    }
}

namespace BuildingManagementTool.Models
{
    public interface IPropertyCategoryRepository
    {
        Task<IEnumerable<PropertyCategory>> PropertyCategories();
        Task<PropertyCategory> GetById(int id);
        Task<IEnumerable<PropertyCategory>> GetByPropertyId(int? id);
    }
}

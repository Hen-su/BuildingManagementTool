namespace BuildingManagementTool.Models
{
    public interface IPropertyRepository
    {
        Task<IEnumerable<Property>> Properties();
        Task<Property> GetById(int id);
        Task AddProperty(Property property);
        Task AddDefaultCategories(Property property);
        Task DeleteProperty(Property property);
    }
}

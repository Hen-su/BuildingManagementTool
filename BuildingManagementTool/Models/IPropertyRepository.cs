namespace BuildingManagementTool.Models
{
    public interface IPropertyRepository
    {
        Task<IEnumerable<Property>> Properties();
        Task<IEnumerable<Property>> GetByUserId(string id);
        Task<Property> GetById(int id);
    }
}

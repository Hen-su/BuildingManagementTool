namespace BuildingManagementTool.Models
{
    public interface IPropertyImageRepository
    {
        IEnumerable<PropertyImage> PropertyImages { get; }
        Task<PropertyImage> GetById(int id);
        Task<IEnumerable<PropertyImage>> GetByPropertyId(int id);
        Task AddPropertyImage(PropertyImage propertyImage);
        Task DeletePropertyImage(PropertyImage propertyImage);
        Task DeleteByPropertyId(int id);
    }
}

namespace BuildingManagementTool.Models
{
    public interface IPropertyImageRepository
    {
        IEnumerable<PropertyImage> PropertyImages { get; }
        Task<PropertyImage> GetById(int id);
        Task<List<PropertyImage>> GetByPropertyId(int id);
        Task AddPropertyImage(PropertyImage propertyImage);
        Task DeletePropertyImage(PropertyImage propertyImage);
        Task DeleteByPropertyId(int id);
        Task<PropertyImage> GetByFileName(int id, string fileName);
        Task SetDisplayImage(PropertyImage propertyImage);
        Task RemoveDisplayImage(int id);
        Task UpdateByList(List<PropertyImage> propertyImages);
    }
}

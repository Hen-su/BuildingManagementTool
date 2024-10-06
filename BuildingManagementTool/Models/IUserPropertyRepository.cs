namespace BuildingManagementTool.Models
{
    public interface IUserPropertyRepository
    {
        Task<IEnumerable<UserProperty>> GetByUserId(string userId);
        Task AddUserProperty(UserProperty userProperty);
        Task DeleteByPropertyId(int id);
        Task<IEnumerable<UserProperty>> GetByPropertyId(int id);
        Task DeleteByUserIdAndPropertyId(int id, string userId);
    }
}

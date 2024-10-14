namespace BuildingManagementTool.Models
{
    public interface IUserPropertyRepository
    {
        Task<IEnumerable<UserProperty>> GetByUserId(string userId);
        Task AddUserProperty(UserProperty userProperty);
        Task DeleteByPropertyId(int id);
        Task<IEnumerable<UserProperty>> GetByPropertyId(int id);
        Task<UserProperty> GetByPropertyIdAndUserId(int id, string userId);
        Task DeleteUserProperty(UserProperty userProperty);
        Task<string> GetManagerUserIdByPropertyId(int id);
    }
}

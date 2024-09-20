namespace BuildingManagementTool.Models
{
    public interface IUserPropertyRepository
    {
        Task<IEnumerable<UserProperty>> GetByUserId(string userId);
        Task AddUserProperty(UserProperty userProperty);
    }
}

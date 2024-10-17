namespace BuildingManagementTool.Models
{
    public interface ISASTokenHandler
    {
        Task<string> GetContainerSasTokenFromSession(string containerName, string role);
    }
}

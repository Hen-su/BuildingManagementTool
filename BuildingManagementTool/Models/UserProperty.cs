namespace BuildingManagementTool.Models
{
    public class UserProperty
    {
        public int UserPropertyId { get; set; }
        public string UserId { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }
}

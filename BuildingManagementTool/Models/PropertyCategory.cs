using Microsoft.Identity.Client;

namespace BuildingManagementTool.Models
{
    public class PropertyCategory
    {
        public int PropertyCategoryId { get; set; }
        public int PropertyId {  get; set; }
        public int CategoryId { get; set; }
        public Property Property { get; set; }
        public Category Category { get; set; }
    }
}

namespace BuildingManagementTool.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public bool IsDisplay {  get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }
}

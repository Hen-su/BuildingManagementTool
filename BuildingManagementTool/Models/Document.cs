using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string BlobName { get; set; }
        [Required]
        public string ContentType { get; set; }
        [Required]
        public long FileSize { get; set; }
        [Required]
        public DateTime UploadDate { get; set; }
    }
}

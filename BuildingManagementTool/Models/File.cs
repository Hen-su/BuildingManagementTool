﻿using Microsoft.Identity.Client;

namespace BuildingManagementTool.Models
{
    public class File
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
    }
}

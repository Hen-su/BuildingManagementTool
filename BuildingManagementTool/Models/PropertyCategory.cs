﻿using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.Models
{
    public class PropertyCategory
    {
        public int PropertyCategoryId { get; set; }
        [Required]
        public int PropertyId {  get; set; }
        public int? CategoryId { get; set; }
        [StringLength(50)]
        public string? CustomCategory {  get; set; }
        public Property Property { get; set; }
        public Category Category { get; set; }

        public string? Color { get; set; }
    }
}

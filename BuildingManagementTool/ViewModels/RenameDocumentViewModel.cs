using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class RenameDocumentViewModel
    {
        [BindNever]
        [ValidateNever]
        public Document Document { get; set; }
        [Required(ErrorMessage = "Filename cannot be empty")]
        [StringLength(50, ErrorMessage = "Filename cannot exceed 50 characters")]
        public string FileName { get; set; }
        public RenameDocumentViewModel() { }
    }
}

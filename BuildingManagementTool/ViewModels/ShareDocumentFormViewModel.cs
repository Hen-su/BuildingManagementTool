using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class ShareDocumentFormViewModel
    {
        [BindNever]
        [ValidateNever]
        public Document Document { get; set; }
        [ValidateNever]
        public string Url { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public ShareDocumentFormViewModel() { }
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class ShareDocumentUrlViewModel
    {
        public string FileName { get; set; }
        public string Url { get; set; }
    }
}

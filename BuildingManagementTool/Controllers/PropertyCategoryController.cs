using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManagementTool.Controllers
{
    [Authorize]
    public class PropertyCategoryController : Controller
    {
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IDocumentRepository _documentRepository; 

        public PropertyCategoryController(IPropertyCategoryRepository propertyCategoryRepository, IDocumentRepository documentRepository)
        {
            _propertyCategoryRepository = propertyCategoryRepository;
            _documentRepository = documentRepository; 
        }

        public async Task<IActionResult> Index(int id)
        {
            id = 1;
            var categories = await _propertyCategoryRepository.GetByPropertyId(id);
            var img = "/imgs/sample-house.jpeg";

            var documentsByCategory = new Dictionary<int, List<Document>>();

            foreach (var category in categories)
            {
                // Fetch documents by category id
                var documents = await _documentRepository.GetDocumentsByCategoryId(category.PropertyCategoryId);
                documentsByCategory[category.PropertyCategoryId] = documents;
            }

            var viewModel = new CategoryViewModel(categories, img)
            {
                CategoryDocuments = documentsByCategory // Add the document data to the ViewModel
            };

            return View(viewModel);
        }
    }
}

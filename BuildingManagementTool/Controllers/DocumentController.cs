using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Authorization;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BuildingManagementTool.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;

        public DocumentController(IDocumentRepository fileRepository, IPropertyCategoryRepository propertyCategoryRepository)
        {
            _documentRepository = fileRepository;
            _propertyCategoryRepository = propertyCategoryRepository;
        }

        public async Task<IActionResult> Index(int id)
        {
            var currentCategory = await _propertyCategoryRepository.GetById(id);
            IEnumerable<Document> documents = _documentRepository.AllDocuments.Where(d => d.PropertyCategoryId == id).ToList();

            var viewModel = new DocumentViewModel(documents, currentCategory);
            return PartialView("_DocumentIndex", viewModel);
        }

        public async Task<IActionResult> UpdateList(int id)
        {
            var currentCategory = await _propertyCategoryRepository.GetById(id);
            IEnumerable<Document> documents = _documentRepository.AllDocuments.Where(d => d.PropertyCategoryId == id).ToList();
            return PartialView("_DocumentList", documents);
        }

        [HttpGet]
        public async Task<IActionResult> UploadFormPartial(int id)
        {
            var propertyCategory = await _propertyCategoryRepository.GetById(id);
            return PartialView("_UploadForm", propertyCategory);
        }

        [HttpGet]
        public IActionResult GetDocumentOptions(int documentId)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == documentId);
            if (document == null)
            {
                return NotFound();
            }
            return PartialView("_DocumentOptions", document);
        }
    }
}

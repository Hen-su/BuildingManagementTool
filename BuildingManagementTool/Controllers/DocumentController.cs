using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Authorization;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BuildingManagementTool.Controllers
{
    [Authorize]
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
            if (currentCategory == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = "The selected category was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            IEnumerable<Document> documents = _documentRepository.AllDocuments.Where(d => d.PropertyCategoryId == id).ToList();

            var viewModel = new DocumentViewModel(documents, currentCategory);
            return PartialView("_DocumentIndex", viewModel);
        }

        public async Task<IActionResult> UpdateList(int id)
        {
            var currentCategory = await _propertyCategoryRepository.GetById(id);
            if (currentCategory == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = "The selected category was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            IEnumerable<Document> documents = _documentRepository.AllDocuments.Where(d => d.PropertyCategoryId == id).ToList();
            return PartialView("_DocumentList", documents);
        }

        [HttpGet]
        public async Task<IActionResult> UploadFormPartial(int id)
        {
            var currentCategory = await _propertyCategoryRepository.GetById(id);
            if (currentCategory == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = "The selected category was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            return PartialView("_UploadForm", currentCategory);
        }

        [HttpGet]
        public IActionResult GetDocumentOptions(int documentId)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == documentId);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Metadata Not Found",
                    Detail = "The File MetaData was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            return PartialView("_DocumentOptions", document);
        }
    }
}

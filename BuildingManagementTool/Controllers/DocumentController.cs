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

        [HttpGet]
        public async Task<IActionResult> AddNoteFormPartial(int id)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == id);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Document Not Found",
                    Detail = "The document was not found.",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }

            return PartialView("_AddNoteForm", document); 
        }

        [HttpPost]
        public async Task<IActionResult> AddNoteToDocument(int documentId, string note)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == documentId);
            if (document == null)
            {
                return NotFound(new { success = false, message = "Document not found." });
            }

            // Adding the note to the document
            document.Note = note;

            // Updating the document in the repository
            await _documentRepository.UpdateDocumentAsync(document);

            return Json(new { success = true, message = "Note added successfully!" });
        }


        [HttpGet]
        public async Task<IActionResult> GetDocumentNotesByCategory(int propertyCategoryId)
        {
            // Retrieve the current property category
            var currentCategory = await _propertyCategoryRepository.GetById(propertyCategoryId);
            if (currentCategory == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = "The selected property category was not found.",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }

            // Get all documents for the given property category
            IEnumerable<Document> documents = _documentRepository.AllDocuments
                .Where(d => d.PropertyCategoryId == propertyCategoryId).ToList();

            // Check if documents are available
            if (!documents.Any())
            {
                return NotFound(new { success = false, message = "No documents found for this property category." });
            }

            // Return the partial view with the list of documents and their notes
            return PartialView("_DocumentNotes", documents);
        }




        [HttpGet]
        public async Task<IActionResult> NoteDeleteConfFormPartial(int id)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == id);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Document Not Found",
                    Detail = "The document was not found.",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }

            return PartialView("_NoteDeleteConfForm", document);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNoteFromDocument(int documentId)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == documentId);
            if (document == null)
            {
                return NotFound(new { success = false, message = "Document not found." });
            }
        
            // Deleting only the inactive note to the document
            if (!document.IsActiveNote)
            {
                document.Note = null;
            }
            // Updating the document in the repository
            await _documentRepository.UpdateDocumentAsync(document);

            return Json(new { success = true, message = "Note deleted successfully!" });
        }



        [HttpGet]
        public IActionResult ActiveNotesWarning()
        {
            // Return the partial view
            return PartialView("_ActiveNotesWarning");
        }

        [HttpPost]
        public async Task<IActionResult> SetActiveNote(int documentId, bool isActive)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == documentId);
            if (document == null)
            {
                return Json(new { success = false, message = "Document not found." });
            }

            // Update the IsActiveNote status
            document.IsActiveNote = isActive;

            // Ensure no more than 2 active notes
            var activeNotesCount = _documentRepository.AllDocuments.Count(d => d.IsActiveNote);
            if (activeNotesCount > 2)
            {
                return Json(new { success = false, message = "Exceeded active notes limit." });
            }

            await _documentRepository.UpdateDocumentAsync(document);

            return Json(new { success = true, message = "Active note status updated!" });
        }

        [HttpGet]
        public async Task<IActionResult> DocumentRenameFormPartial(int id)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == id);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Document Not Found",
                    Detail = "The document was not found.",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }

            return PartialView("_DocumentRenameForm", document);
        }

        [HttpPost]
        public async Task<IActionResult> DocumentFileNameRename(int documentId, string filename)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == documentId);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Document Not Found",
                    Detail = "The document was not found.",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }

            document.FileName = filename;
            await _documentRepository.UpdateDocumentAsync(document);

            return Json(new { success = true, message = "Document renamed successfully!" });
        }


    }
}

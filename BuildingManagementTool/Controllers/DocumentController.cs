using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Authorization;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BuildingManagementTool.Services;
using SendGrid.Helpers.Mail.Model;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using BuildingManagementTool.Services.Authorization;


namespace BuildingManagementTool.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IEmailSender _emailSender;
        private readonly IRazorViewToStringRenderer _viewToStringRenderer;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserPropertyRepository _userPropertyRepository;
        private readonly IAuthorizationService _authorizationService;

        public DocumentController(IDocumentRepository fileRepository, IPropertyCategoryRepository propertyCategoryRepository, 
            IEmailSender emailSender, IRazorViewToStringRenderer viewToStringRenderer, UserManager<ApplicationUser> userManager, 
            IUserPropertyRepository userPropertyRepository, IAuthorizationService authorizationService)
        {
            _documentRepository = fileRepository;
            _propertyCategoryRepository = propertyCategoryRepository;
            _emailSender = emailSender;
            _viewToStringRenderer = viewToStringRenderer;
            _userManager = userManager;
            _userPropertyRepository = userPropertyRepository;
            _authorizationService = authorizationService;
        }

        private async Task<AuthorizationResult> CheckAuthorizationByPropertyId(int id)
        {
            // Create a requirement instance with the actual property ID
            var requirement = new UserPropertyManagerRequirement(id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, requirement);
            return authorizationResult;
        }

        private async Task<string> GetRoleName(int propertyId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var userproperty = await _userPropertyRepository.GetByPropertyIdAndUserId(propertyId, userId);

            return userproperty.Role.Name;
        }

        private async Task<DocumentViewModel> CreateDocumentViewModel(int id)
        {
            var currentCategory = await _propertyCategoryRepository.GetById(id);
            if (currentCategory == null)
            {
                return null;
            }

            IEnumerable<Document> documents = _documentRepository.AllDocuments.Where(d => d.PropertyCategoryId == id).ToList();

            var roleName = await GetRoleName(currentCategory.PropertyId);

            var viewModel = new DocumentViewModel(documents, currentCategory, roleName);
            return viewModel;
        }

        public async Task<IActionResult> Index(int id)
        {
            var viewModel = await CreateDocumentViewModel(id);
            if (viewModel == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = "The selected category was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
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
        public async Task<IActionResult> GetDocumentOptions(int documentId)
        {
            var document = await _documentRepository.GetById(documentId);
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

            var roleName = await GetRoleName(document.PropertyCategory.PropertyId);

            var viewModel = new DocumentOptionsViewModel(document, roleName);
            return PartialView("_DocumentOptions", viewModel);
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
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var viewModel = new AddNoteViewModel { Document = document };
            viewModel.Note = document.Note;
            return PartialView("_AddNoteForm", viewModel); 
        }

        
        [HttpPost]
        public async Task<IActionResult> AddNoteToDocument(int documentId, AddNoteViewModel viewModel)
        {
            var document = _documentRepository.AllDocuments.FirstOrDefault(d => d.DocumentId == documentId);
            if (document == null)
            {
                return NotFound(new { success = false, message = "Document not found." });
            }
            
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            
            if (!ModelState.IsValid) 
            {
                viewModel.Document = document;  
                return PartialView("_AddNoteForm", viewModel);
            }
            // Adding the note to the document
            document.Note = viewModel.Note.Trim();

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
            IEnumerable<Document> documents = await _documentRepository.GetByPropertyCategoryId(currentCategory.PropertyCategoryId);

            // Check if documents are available
            if (!documents.Any())
            {
                return NotFound(new { success = false, message = "No documents found for this property category." });
            }
            var roleName = await GetRoleName(currentCategory.PropertyId);
            var viewModel = new DocumentNotesViewModel(documents, roleName);
            // Return the partial view with the list of documents and their notes
            return PartialView("_DocumentNotes", viewModel);
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
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
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
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Deleting only the inactive note to the document
            if (!document.IsActiveNote)
            {
                document.Note = null;
                // Updating the document in the repository
                await _documentRepository.UpdateDocumentAsync(document);
            }
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
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var currentCategory = await _propertyCategoryRepository.GetById(document.PropertyCategoryId);
            if (currentCategory == null)
            {
                return Json(new { success = false, message = "Property category not found." });
            }

            var activeNotesCount = _documentRepository.AllDocuments
                .Count(d => d.PropertyCategoryId == currentCategory.PropertyCategoryId && d.IsActiveNote);

            if (activeNotesCount < 2 || !isActive) 
            {
              
                document.IsActiveNote = isActive;
                await _documentRepository.UpdateDocumentAsync(document);

                return Json(new { success = true, message = "Active note status updated!" });
            }
            else
            {
                return Json(new { success = false, message = "Exceeded active notes limit." });
            }
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
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var viewModel = new RenameDocumentViewModel { Document = document, FileName = document.FileName };
            return PartialView("_DocumentRenameForm", viewModel);
        }

        
        [HttpPost]
        public async Task<IActionResult> DocumentFileNameRename(int documentId, RenameDocumentViewModel viewModel)
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
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                viewModel.Document = document;
                return PartialView("_DocumentRenameForm", viewModel);
            }
            document.FileName = viewModel.FileName;
            await _documentRepository.UpdateDocumentAsync(document);

            return Json(new { success = true, message = "Document renamed successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentShareUrlPartial(int id)
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
            var viewModel = new ShareDocumentFormViewModel { Document = document };
            return PartialView("_GetDocumentShareUrl", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ShareDocumentUrl(int documentId, ShareDocumentFormViewModel viewModel)
        {
            var document = await _documentRepository.GetById(documentId);
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

            if (!ModelState.IsValid)
            {
                viewModel.Document = document;
                return PartialView("_GetDocumentShareUrl", viewModel);
            }

            var model = new ShareDocumentUrlViewModel
            {
                FileName = document.FileName,
                Url = viewModel.Url
            };

            var emailContent = await _viewToStringRenderer.RenderViewToStringAsync("Shared/EmailTemplates/ShareDocumentUrlEmail", model, HttpContext);

            try
            {
                await _emailSender.SendEmailAsync(viewModel.Email, "Document Share Link", emailContent);
            }
            catch (Exception ex)
            {
            
                return Json(new { success = false, message = "Error sending email: " + ex.Message });
            }

            return Json(new { success = true, message = "Document link shared successfully!" });
        }


    }
}

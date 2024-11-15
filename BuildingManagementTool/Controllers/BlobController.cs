﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using BuildingManagementTool.Services.Authorization;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Azure;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace BuildingManagementTool.Controllers
{
    [Authorize]
    public class BlobController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IBlobService _blobService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserPropertyRepository _userPropertyRepository;
        private readonly IAuthorizationService _authorizationService;
        public BlobController(IBlobService blobService, IDocumentRepository documentRepository, IPropertyCategoryRepository propertyCategoryRepository, 
            UserManager<ApplicationUser> userManager, ICategoryRepository categoryRepository, IPropertyRepository propertyRepository, 
            IUserPropertyRepository userPropertyRepository, IAuthorizationService authorizationService)
        {
            _blobService = blobService;
            _documentRepository = documentRepository;
            _propertyCategoryRepository = propertyCategoryRepository;
            _userManager = userManager;
            _categoryRepository = categoryRepository;
            _propertyRepository = propertyRepository;
            _userPropertyRepository = userPropertyRepository;
            _authorizationService = authorizationService;
        }

        private async Task<string> GetRoleName(int propertyId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userProperty = await _userPropertyRepository.GetByPropertyIdAndUserId(propertyId, user.Id);
            var role = userProperty.Role.Name;
            return role;
        }

        private async Task<AuthorizationResult> CheckAuthorizationByPropertyId(int id)
        {
            // Create a requirement instance with the actual property ID
            var requirement = new UserPropertyManagerRequirement(id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, requirement);
            return authorizationResult;
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IList<IFormFile> files, int id)
        {
            var propertyCategory = await _propertyCategoryRepository.GetById(id);
            if (propertyCategory == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "The selected category was not found in the database"
                });
            }
            var authorizationResult = await CheckAuthorizationByPropertyId(propertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var role = "Manager";
            //Check submitted file list isn't empty
            if (files == null || files.Count == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No file was uploaded. Please select a file before submitting."
                });
            }
            var user = await _userManager.GetUserAsync(User);
            var containerName = "userid-" + user.Id;

            //Check if a blob with the same name exists
            foreach (var file in files)
            {
                string blobName;
                if (propertyCategory.CategoryId != null)
                {
                    blobName = $"{propertyCategory.Property.PropertyName}/{propertyCategory.Category.CategoryName}/{file.FileName}".Trim();
                }
                else
                {
                    blobName = $"{propertyCategory.Property.PropertyName}/{propertyCategory.CustomCategory}/{file.FileName}".Trim();
                }

                bool blobExists = await _blobService.BlobExistsAsync(containerName, blobName, role);
                if (blobExists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, new
                    {
                        success = false,
                        message = $"A blob with the same name already exists. {blobName}"
                    });
                }
                //Upload to blob storage
                using (var stream = file.OpenReadStream())
                {
                    BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType };
                    bool isUploaded = await _blobService.UploadBlobAsync(containerName, blobName, stream, blobHttpHeaders, role);
                    //Check if upload was successful
                    if (!isUploaded)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new
                        {
                            success = false,
                            message = "Failed to upload the file to blob storage."
                        });
                    }
                }
                /*Switch Case for the image url using the content type*/
                string imageUrl;

                switch (file.ContentType)
                {
                    case "image/jpeg":
                    case "image/jpg":
                        imageUrl = "/imgs/jpg.svg";
                        break;

                    case "image/png":
                        imageUrl = "/imgs/png.png";
                        break;

                    case "application/pdf":
                        imageUrl = "/imgs/pdf.svg";
                        break;

                    case "application/msword":
                    case "application/vnd.openxmlformats-officedocument.wordprocessingml.document": // .docx
                        imageUrl = "/imgs/file-word.svg";
                        break;

                    case "text/plain":
                        imageUrl = "/imgs/txt.svg";
                        break;

                    default:
                        imageUrl = "/imgs/generic.png"; // Fallback image for unknown content types
                        break;
                }

                //Create metadata
                var metadata = new Models.Document
                {
                    FileName = file.FileName,
                    BlobName = blobName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.UtcNow,
                    FileImageUrl = imageUrl,
                    PropertyCategoryId = propertyCategory.PropertyCategoryId
                };
                //Add record to SQL Server
                try
                {
                    await _documentRepository.AddDocumentData(metadata);
                }
                //Remove blob if metadata was not added
                catch
                {
                    await _blobService.DeleteBlobAsync(containerName, blobName, role);

                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        success = false,
                        message = "Failed to save metadata in database. The uploaded blob will be removed."
                    });
                }
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBlob(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var containerName = "userid-" + user.Id;
            //Check metadata exists
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The File MetaData was not found"
                });
            }
            var authorizationResult = await CheckAuthorizationByPropertyId(document.PropertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var role = "Manager";
            //Delete from blob storage
            bool isDeleted = await _blobService.DeleteBlobAsync(containerName, document.BlobName, role);
            //Check if succcessful
            if (!isDeleted)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred when deleting the file"
                });
            }
            //Delete metadata if blob is deleted
            await _documentRepository.DeleteDocumentData(document);
            return Json(new { success = true }); ;
        }

        public async Task<IActionResult> DeleteConfirmationPartial(int id)
        {
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The File MetaData was not found"
                });
            }
            return PartialView("_DeleteConfirmation", document);
        }

        public IActionResult PDFViewerPartial(string blobUrl)
        {
            return PartialView("_PDFViewer", blobUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            
            //Check metadata exists
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The File MetaData was not found"
                });
            }
            var propertyId = document.PropertyCategory.PropertyId;
            var managerId = await _userPropertyRepository.GetManagerUserIdByPropertyId(propertyId);
            var containerName = "userid-" + managerId;

           var role = await GetRoleName(propertyId);
            //Download blob
            var stream = await _blobService.DownloadBlobAsync(containerName, document.BlobName, role);
            if (stream == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The File was not found in blob storage"
                });
            }

            return File(stream, document.ContentType, document.FileName);
        }
        //Renders Partial Views to Modal container
        public IActionResult VideoPlayerPartial(string blobUrl)
        {
            return PartialView("_VideoPlayer", blobUrl);
        }

        public IActionResult ImageViewerPartial(string blobUrl)
        {
            return PartialView("_ImageViewer", blobUrl);
        }
        //Gets Url of blobs and handles rendering based on content type
        public async Task<IActionResult> RenderFile(int id)
        {
            //Check if metadata exists
            var document = await _documentRepository.GetById(id);
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
            var propertyId = document.PropertyCategory.PropertyId;
            var managerId = await _userPropertyRepository.GetManagerUserIdByPropertyId(propertyId);
            var role = await GetRoleName(propertyId);
            var containerName = "userid-" + managerId;
            var blobName = document.BlobName;
            //Get URL of blob by name and container
            var blobUrl = await _blobService.GetBlobUrlAsync(containerName, blobName, role);
            if (blobUrl == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "File Not Found",
                    Detail = "The file was not found in blob storage",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            //Handle rendering of blob by MIME content type
            if (document.ContentType == "application/pdf")
            {
                return PDFViewerPartial(blobUrl);
            }
            else if (document.ContentType.StartsWith("video"))
            {
                return VideoPlayerPartial(blobUrl);
            }
            else if (document.ContentType.StartsWith("image"))
            {
                return ImageViewerPartial(blobUrl);
            }
            else
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Unsupported File Type",
                    Detail = "The content type is not supported for rendering",
                    Status = StatusCodes.Status415UnsupportedMediaType
                };
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, problemDetails);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePropertyCategory(int id)
        {
            var propertyCategory = await _propertyCategoryRepository.GetById(id);
            if (propertyCategory == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "Could not find matching category"
                });
            }
            var authorizationResult = await CheckAuthorizationByPropertyId(propertyCategory.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var role = "Manager";
            var documents = _documentRepository.AllDocuments.Where(d => d.PropertyCategoryId == propertyCategory.PropertyCategoryId).ToList();
            if (documents.Any())
            {
                foreach (var document in documents)
                {
                    await _documentRepository.DeleteDocumentData(document);
                }
            }
            await _propertyCategoryRepository.DeletePropertyCategory(propertyCategory);

            var user = await _userManager.GetUserAsync(User);
            var containerName = "userid-" + user.Id;
            string prefix;
            if (propertyCategory.CategoryId != null)
            {
                prefix = $"{propertyCategory.Property.PropertyName}/{propertyCategory.Category.CategoryName}".Trim();
            }
            else
            {
                prefix = $"{propertyCategory.Property.PropertyName}/{propertyCategory.CustomCategory}".Trim();
            }
            var deleteSuccess = await _blobService.DeleteByPrefix(containerName, prefix, role);
            if (!deleteSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while deleting the category, Please try again"
                });
            }
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentShareUrl(int id)
        {
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,        
                    message = "The File MetaData was not found"
                });
            }
            var propertyId = document.PropertyCategory.PropertyId;
            var managerId = await _userPropertyRepository.GetManagerUserIdByPropertyId(propertyId);
            var containerName = "userid-" + managerId;
            var role = await GetRoleName(propertyId);
            var blobUrl = await _blobService.GetBlobUrlAsync(containerName, document.BlobName, role);
            if (string.IsNullOrEmpty(blobUrl))
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The File was not found in blob storage"
                });
            }

            return Ok(new
            {
                success = true,
                url = blobUrl
            });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _propertyRepository.GetById(id);
            if (property == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property was not found"
                });
            }
            var authorizationResult = await CheckAuthorizationByPropertyId(property.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var role = "Manager";
            await _propertyRepository.DeleteProperty(property);
            var user = await _userManager.GetUserAsync(User);
            var containerName = "userid-" + user.Id;
            string prefix = $"{property.PropertyName}".Trim();
            var deleteSuccess = await _blobService.DeleteByPrefix(containerName, prefix, role);
            if (!deleteSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while deleting the property, Please try again"
                });
            }
            return Json(new { success = true });
        }

    }
}
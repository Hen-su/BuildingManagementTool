using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace BuildingManagementTool.Controllers
{
    public class UserPropertyController : Controller
    {
        private readonly IUserPropertyRepository _userPropertyRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IBlobService _blobService;
        private readonly IPropertyImageRepository _propertyImageRepository;

        public UserPropertyController(IUserPropertyRepository userPropertyRepository, IPropertyRepository propertyRepository, 
            UserManager<ApplicationUser> userManager, IDocumentRepository documentRepository, 
            IPropertyCategoryRepository propertyCategoryRepository, IBlobService blobService, IPropertyImageRepository propertyImageRepository)
        {
            _userPropertyRepository = userPropertyRepository;
            _propertyRepository = propertyRepository;
            _userManager = userManager;
            _documentRepository = documentRepository;
            _propertyCategoryRepository = propertyCategoryRepository;
            _blobService = blobService;
            _propertyImageRepository = propertyImageRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var containerName = "userid-" + user.Id;
            if (user == null) 
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            var userId = user.Id;
            var propertyList = await _userPropertyRepository.GetByUserId(userId);

            var viewmodelList = new List<PropertyViewModel>();
            foreach (var property in propertyList)
            {
                var propertyImages = await _propertyImageRepository.GetByPropertyId(property.PropertyId);
                var displayImage = propertyImages.FirstOrDefault(img => img.IsDisplay);
                if (displayImage != null)
                {
                    var url = await _blobService.GetBlobUrlAsync(containerName, displayImage.BlobName);
                    viewmodelList.Add(new PropertyViewModel(property, url.ToString()));
                }
                else
                {
                    viewmodelList.Add(new PropertyViewModel(property, null));
                }
            }
            return View(viewmodelList);
        }

        public async Task<IActionResult> PropertyFormPartial()
        {
            var viewModel = new PropertyFormViewModel(null);
            return PartialView("_PropertyForm", viewModel);
        }

        public async Task<IActionResult> AddProperty(string name)
        {
            if (name == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    success = false,
                    message = "The property name cannot be null"
                });
            }
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            var userId = user.Id;

            var newProperty = new Property
            {
                PropertyName = name
            };

            await _propertyRepository.AddProperty(newProperty);

            var newUserProperty = new UserProperty
            {
                PropertyId = newProperty.PropertyId,
                UserId = userId
            };

            await _userPropertyRepository.AddUserProperty(newUserProperty);
            await _propertyRepository.AddDefaultCategories(newProperty);
            return Json(new { success = true });
        }

        public async Task<IActionResult> UpdatePropertyContainer()
        {
            var user = await _userManager.GetUserAsync(User);
            var containerName = "userid-" + user.Id;
            if (user == null)
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            var userId = user.Id;
            var propertyList = await _userPropertyRepository.GetByUserId(userId);

            var viewmodelList = new List<PropertyViewModel>();
            foreach (var property in propertyList)
            {
                var propertyImages = await _propertyImageRepository.GetByPropertyId(property.PropertyId);
                var displayImage = propertyImages.FirstOrDefault(img => img.IsDisplay);
                if (displayImage != null)
                {
                    var url = await _blobService.GetBlobUrlAsync(containerName, displayImage.BlobName);
                    viewmodelList.Add(new PropertyViewModel(property, url.ToString()));
                }
                else
                {
                    viewmodelList.Add(new PropertyViewModel(property, null));
                }
            }
            return PartialView("_PropertyContainer", viewmodelList);
        }

        public async Task<IActionResult> DeleteConfirmationPartial(int id)
        {
            if (id == null || id == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    success = false,
                    message = "The property id cannot be null"
                });
            }

            var property = await _propertyRepository.GetById(id);
            if (property == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }
            return PartialView("_PropertyDeleteConfirmation", property);
        }

        public async Task<IActionResult> ManagePropertyFormPartial(int id)
        {
            if (id == null || id == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    success = false,
                    message = "The property id cannot be null"
                });
            }

            var property = await _propertyRepository.GetById(id);
            if (property == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }
            var imageList = new List<Dictionary<int, List<string>>>();

            var images = await _propertyImageRepository.GetByPropertyId(property.PropertyId);
            if (images.Any())
            {
                var user = await _userManager.GetUserAsync(User);
                var containerName = "userid-" + user.Id;

                var prefix = $"{property.PropertyName}/images/".Trim();
                var blobs = await _blobService.GetBlobUrisByPrefix(containerName, prefix);
            
                if (blobs != null && blobs.Any())
                {
                    foreach (var kvp in blobs)
                    {
                        var updatedList = new List<string>
                        {
                            kvp.Value[0],
                            kvp.Value[1],
                            images.FirstOrDefault(i => i.FileName == kvp.Value[0]).IsDisplay.ToString()
                        };
                        imageList.Add(new Dictionary<int, List<string>> { { kvp.Key, updatedList } });
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    imageList.Add(new Dictionary<int, List<string>> { { i, new List<string> { null } } });
                }
            }
            
            var viewmodel = new ManagePropertyFormViewModel(imageList, property) { PropertyName = property.PropertyName };
            return PartialView("_ManagePropertyForm", viewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> ManagePropertyFormSubmit(int id, ManagePropertyFormViewModel formViewModel, string? selectedFileName)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    // Log the error message or display it as needed
                    Console.WriteLine(error.ErrorMessage);
                }
                return PartialView("_ManagePropertyForm", formViewModel);
            }

            var currentProperty = await _propertyRepository.GetById(id);
            var user = await _userManager.GetUserAsync(User);
            var containerName = "userid-" + user.Id;
            //Replace copy and replace blobs
            if (currentProperty.PropertyName != formViewModel.PropertyName)
            {
                var newDirectory = formViewModel.PropertyName.Trim();
                string oldDirectory = currentProperty.PropertyName.Trim();

                currentProperty.PropertyName = formViewModel.PropertyName;
                await _propertyRepository.UpdateProperty(currentProperty);

                var documents = await _documentRepository.GetByPropertyId(currentProperty.PropertyId);
                foreach (var document in documents)
                {
                    var categoryFileName = document.BlobName.Substring(document.BlobName.IndexOf('/'));
                    document.BlobName = newDirectory + categoryFileName;
                    await _blobService.RenameBlobDirectory(containerName, oldDirectory, newDirectory);
                }
                await _documentRepository.UpdateByList(documents);
            }

            //Upload Images
            if (formViewModel.Images != null && formViewModel.Images.Any())
            {
                var imagesList = await _propertyImageRepository.GetByPropertyId(currentProperty.PropertyId);
                foreach (var image in formViewModel.Images)
                {
                    if (imagesList.Any(i => i.FileName == image.FileName))
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = $"A blob with the same name already exists. {image.FileName}"
                        });
                    }
                    var blobName = $"{currentProperty.PropertyName}/images/{image.FileName}".Trim();
                    bool blobExists = await _blobService.BlobExistsAsync(containerName, blobName);
                    if (blobExists)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = $"A blob with the same name already exists. {blobName}"
                        });
                    }
                    //Upload to blob storage
                    using (var stream = image.OpenReadStream())
                    {
                        BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders { ContentType = image.ContentType };
                        bool isUploaded = await _blobService.UploadBlobAsync(containerName, blobName, stream, blobHttpHeaders);
                        //Check if upload not successful
                        if (!isUploaded)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new
                            {
                                success = false,
                                message = $"Failed to upload the file to blob storage. {blobName}"
                            });
                        }
                    }
                    var file = new PropertyImage { FileName = image.FileName, BlobName = blobName, IsDisplay = false, PropertyId = currentProperty.PropertyId };
                    await _propertyImageRepository.AddPropertyImage(file);
                }
            }
            if (selectedFileName != null && selectedFileName != "")
            {
                var displayImage = await _propertyImageRepository.GetByFileName(selectedFileName);
                if (displayImage != null) 
                {
                    if (!displayImage.IsDisplay)
                    {
                        await _propertyImageRepository.SetDisplayImage(displayImage);
                    }
                }
            }
            else
            {
                await _propertyImageRepository.RemoveDisplayImage(currentProperty.PropertyId);
            }
            return Json(new { success = true });
        }
    }
}

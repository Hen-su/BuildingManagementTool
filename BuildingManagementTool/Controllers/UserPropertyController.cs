using Azure.Storage.Blobs.Models;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using BuildingManagementTool.Services.Authorization;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Policy;
using Property = BuildingManagementTool.Models.Property;

namespace BuildingManagementTool.Controllers
{
    [Authorize]
    public class UserPropertyController : Controller
    {
        private readonly IUserPropertyRepository _userPropertyRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IBlobService _blobService;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IInvitationService _invitationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserPropertyController(IUserPropertyRepository userPropertyRepository, IPropertyRepository propertyRepository, 
            UserManager<ApplicationUser> userManager, IDocumentRepository documentRepository, 
            IPropertyCategoryRepository propertyCategoryRepository, IBlobService blobService, IPropertyImageRepository propertyImageRepository, 
            RoleManager<IdentityRole> roleManager, IInvitationService invitationService, IAuthorizationService authorizationService)
        {
            _userPropertyRepository = userPropertyRepository;
            _propertyRepository = propertyRepository;
            _userManager = userManager;
            _documentRepository = documentRepository;
            _propertyCategoryRepository = propertyCategoryRepository;
            _blobService = blobService;
            _propertyImageRepository = propertyImageRepository;
            _roleManager = roleManager;
            _invitationService = invitationService;
            _authorizationService = authorizationService;
        }

        private async Task<AuthorizationResult> CheckAuthorizationByPropertyId(int id)
        {
            // Create a requirement instance with the actual property ID
            var requirement = new UserPropertyManagerRequirement(id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, requirement);
            return authorizationResult;
        }

        private async Task<List<PropertyViewModel>> CreatePropertyViewModels()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return null;
            }
            var userId = user.Id;
            var propertyList = await _userPropertyRepository.GetByUserId(userId);

            var viewmodelList = new List<PropertyViewModel>();
            foreach (var property in propertyList)
            {
                var propertyImages = await _propertyImageRepository.GetByPropertyId(property.PropertyId);
                var displayImage = propertyImages.FirstOrDefault(img => img.IsDisplay);

                var managerId = await _userPropertyRepository.GetManagerUserIdByPropertyId(property.PropertyId);

                if (displayImage != null)
                {
                    var containerName = "userid-" + managerId;
                    var url = await _blobService.GetBlobUrlAsync(containerName, displayImage.BlobName, property.Role.Name);
                    viewmodelList.Add(new PropertyViewModel(property, url.ToString()));
                }
                else
                {
                    viewmodelList.Add(new PropertyViewModel(property, null));
                }
            }
            return viewmodelList;
        }

        public async Task<IActionResult> Index()
        {
            var viewModelList = await CreatePropertyViewModels();
            if (viewModelList == null)
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            return View(viewModelList);
        }

        public async Task<IActionResult> PropertyFormPartial()
        {
            var viewModel = new PropertyFormViewModel(null);
            return PartialView("_PropertyForm", viewModel);
        }

        public async Task<IActionResult> AddProperty(PropertyFormViewModel viewmodel)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_PropertyForm", viewmodel);
            }
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var newProperty = new Property
            {
                PropertyName = viewmodel.PropertyName.Trim()
            };

            await _propertyRepository.AddProperty(newProperty);

            var role = await _roleManager.FindByNameAsync("Manager");

            var newUserProperty = new UserProperty
            {
                PropertyId = newProperty.PropertyId,
                UserId = userId,
                RoleId = role.Id
            };

            await _userPropertyRepository.AddUserProperty(newUserProperty);
            await _propertyRepository.AddDefaultCategories(newProperty);
            await _blobService.CreateBlobContainer("userid-"+userId);
            return Json(new { success = true });
        }

        public async Task<IActionResult> UpdatePropertyContainer()
        {
            var viewModelList = await CreatePropertyViewModels();
            if (viewModelList == null)
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            return PartialView("_PropertyContainer", viewModelList);
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

        private async Task<List<Dictionary<int, List<string>>>> GetImageList(Property property)
        {
            var imageList = new List<Dictionary<int, List<string>>>();

            var images = await _propertyImageRepository.GetByPropertyId(property.PropertyId);
            if (images.Any())
            {
                var user = await _userManager.GetUserAsync(User);
                var containerName = "userid-" + user.Id;
                var role = "Manager";

                var prefix = $"{property.PropertyName}/images/".Trim();
                var blobs = await _blobService.GetBlobUrisByPrefix(containerName, prefix, role);

                if (blobs != null && blobs.Any())
                {
                    int dictionaryCount = 0;
                    foreach (var kvp in blobs)
                    {
                        var updatedList = new List<string>
                        {
                            kvp.Value[0],
                            kvp.Value[1],
                            images.FirstOrDefault(i => i.FileName == kvp.Value[0]).IsDisplay.ToString()
                        };
                        imageList.Add(new Dictionary<int, List<string>> { { kvp.Key, updatedList } });
                        dictionaryCount++;
                    }

                    while (dictionaryCount < 5)
                    {
                        imageList.Add(new Dictionary<int, List<string>> { { dictionaryCount, new List<string> { null } } });
                        dictionaryCount++;
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
            return imageList;
        }

        private async Task<ManagePropertyFormViewModel> CreateManagePropertyFormViewModel(int id)
        {
            var property = await _propertyRepository.GetById(id);
            if (property == null)
            {
                return null;
            }
            var user = await _userManager.GetUserAsync(User);
            var imageList = await GetImageList(property);
            var userPropertyList = await _userPropertyRepository.GetByPropertyId(property.PropertyId);
            var emailList = new Dictionary<int, string>();
            if (userPropertyList != null && userPropertyList.Any())
            {
                foreach (var item in userPropertyList)
                {
                    var propertyUser = await _userManager.FindByIdAsync(item.UserId);
                    var email = await _userManager.GetEmailAsync(propertyUser);
                    if (email != User.Identity.Name)
                    {
                        emailList.Add(item.UserPropertyId, email);
                    }
                }
            }
            var addViewerViewModel = new AddViewerViewModel();
            var viewmodel = new ManagePropertyFormViewModel(imageList, property, property.PropertyName, emailList) { AddViewerViewModel = addViewerViewModel };
            return viewmodel;
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
            var authorizationResult = await CheckAuthorizationByPropertyId(id);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var viewmodel = await CreateManagePropertyFormViewModel(id);
            if (viewmodel == null) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "A problem occurred when processing your request. Please try again"
                });
            }
            return PartialView("_ManagePropertyForm", viewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> ManagePropertyFormSubmit(int id, ManagePropertyFormViewModel formViewModel, string? selectedFileName, List<string> filesToRemove)
        {
            var authorizationResult = await CheckAuthorizationByPropertyId(id);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var role = "Manager";
            var user = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid)
            {
                var property = await _propertyRepository.GetById(id);
                var userproperty = await _userPropertyRepository.GetByPropertyIdAndUserId(property.PropertyId, user.Id);
                var imageList = await GetImageList(property);
                formViewModel.CurrentProperty = property;
                formViewModel.ImageUrls = imageList;
                return PartialView("_ManagePropertyForm", formViewModel);
            }

            var currentProperty = await _propertyRepository.GetById(id);
            var currentuserProperty = await _userPropertyRepository.GetByPropertyIdAndUserId(currentProperty.PropertyId, user.Id);
            var containerName = "userid-" + user.Id;
            //Replace copy and replace blobs
            if (currentProperty.PropertyName != formViewModel.PropertyName)
            {
                var newDirectory = formViewModel.PropertyName.Trim();
                string oldDirectory = currentProperty.PropertyName.Trim();

                currentProperty.PropertyName = formViewModel.PropertyName;
                await _propertyRepository.UpdateProperty(currentProperty);

                var documents = await _documentRepository.GetByPropertyId(currentProperty.PropertyId);
                if (documents != null && documents.Count > 0)
                {
                    foreach (var document in documents)
                    {
                        var categoryFileName = document.BlobName.Substring(document.BlobName.IndexOf('/'));
                        document.BlobName = newDirectory + categoryFileName;
                        await _blobService.RenameBlobDirectory(containerName, oldDirectory, newDirectory, role);
                    }
                    await _documentRepository.UpdateByList(documents);
                }
                var images = await _propertyImageRepository.GetByPropertyId(currentProperty.PropertyId);
                if (images.Any())
                {
                    foreach (var image in images)
                    {
                        var categoryFileName = image.BlobName.Substring(image.BlobName.IndexOf('/'));
                        image.BlobName = newDirectory + categoryFileName;
                        await _blobService.RenameBlobDirectory(containerName, oldDirectory, newDirectory, role);
                    }
                    await _propertyImageRepository.UpdateByList(images);
                }
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
                    bool blobExists = await _blobService.BlobExistsAsync(containerName, blobName, currentuserProperty.Role.Name);
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
                        bool isUploaded = await _blobService.UploadBlobAsync(containerName, blobName, stream, blobHttpHeaders, currentuserProperty.Role.Name);
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
            //Update Image to be displayed
            if (selectedFileName != null && selectedFileName != "")
            {
                var displayImage = await _propertyImageRepository.GetByFileName(currentProperty.PropertyId, selectedFileName);
                if (displayImage != null) 
                {
                    if (!displayImage.IsDisplay)
                    {
                        await _propertyImageRepository.SetDisplayImage(displayImage);
                    }
                }
            }
            //Set isDisplay to false if no image is selected
            else
            {
                await _propertyImageRepository.RemoveDisplayImage(currentProperty.PropertyId);
            }

            //Delete image if it was removed from gallery
            if (filesToRemove != null && filesToRemove.Count() > 0)
            {

                var imagesList = await _propertyImageRepository.GetByPropertyId(currentProperty.PropertyId);
                foreach ( var file in filesToRemove)
                {
                    var imageToDelete = imagesList.FirstOrDefault(i => i.FileName == file);
                    if (imageToDelete != null)
                    {
                        await _blobService.DeleteBlobAsync(containerName ,imageToDelete.BlobName, role);
                        await _propertyImageRepository.DeletePropertyImage(imageToDelete);
                        continue;
                    }
                }
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SendInviteEmail(int id, AddViewerViewModel addViewerViewModel)
        {
            var authorizationResult = await CheckAuthorizationByPropertyId(id);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            if (!ModelState.IsValid)
            {
                var viewmodel = await CreateManagePropertyFormViewModel(id);
                viewmodel.AddViewerViewModel = addViewerViewModel;
                return PartialView("_ManagePropertyForm", viewmodel);
            }
            await _invitationService.InviteUserAsync(addViewerViewModel.Email, id);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserProperty(int id, string email)
        {
            if (id == null || id == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    success = false,
                    message = $"Invalid property id"
                });
            }
            if (email == null || email == "")
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    success = false,
                    message = $"Invalid user email"
                });
            }
            var authorizationResult = await CheckAuthorizationByPropertyId(id);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user.Id;
            var userProperty = await _userPropertyRepository.GetByPropertyIdAndUserId(id, userId);
            if (userProperty != null) 
            { 
                await _userPropertyRepository.DeleteUserProperty(userProperty);
            }
            return Json(new { success = true });
        }



        [HttpGet]
        public async Task<IActionResult> SearchPropertyByName(string keyword)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            
            var userId = user.Id;
            var propertyList = await _userPropertyRepository.GetByUserId(userId);

            var viewmodelList = new List<PropertyViewModel>();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                var filterKeyword = keyword.ToLower();
                var matchList = propertyList.Where(p => p.Property.PropertyName.ToLower().Contains(filterKeyword));
                foreach (var property in matchList)
                {
                    var managerId = await _userPropertyRepository.GetManagerUserIdByPropertyId(property.PropertyId);
                    var propertyImages = await _propertyImageRepository.GetByPropertyId(property.PropertyId);
                    var displayImage = propertyImages.FirstOrDefault(img => img.IsDisplay);
                    if (displayImage != null)
                    {
                        var containerName = "userid-" + managerId;
                        var url = await _blobService.GetBlobUrlAsync(containerName, displayImage.BlobName, property.Role.Name);
                        viewmodelList.Add(new PropertyViewModel(property, url.ToString()));
                    }
                    else
                    {
                        viewmodelList.Add(new PropertyViewModel(property, null));
                    }
                }
            }
            else
            {
                foreach (var property in propertyList)
                {
                    var managerId = await _userPropertyRepository.GetManagerUserIdByPropertyId(property.PropertyId);
                    var propertyImages = await _propertyImageRepository.GetByPropertyId(property.PropertyId);
                    var displayImage = propertyImages.FirstOrDefault(img => img.IsDisplay);
                    if (displayImage != null)
                    {
                        var containerName = "userid-" + managerId;
                        var url = await _blobService.GetBlobUrlAsync(containerName, displayImage.BlobName, property.Role.Name);
                        viewmodelList.Add(new PropertyViewModel(property, url.ToString()));
                    }
                    else
                    {
                        viewmodelList.Add(new PropertyViewModel(property, null));
                    }
                }
            }
            return PartialView("_PropertyContainer", viewmodelList);
        }
    }
}

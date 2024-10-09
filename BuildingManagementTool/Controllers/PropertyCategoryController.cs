using BuildingManagementTool.Models;
using BuildingManagementTool.Services.Authorization;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;



namespace BuildingManagementTool.Controllers
{
    [Authorize]
    public class PropertyCategoryController : Controller
    {
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserPropertyRepository _userPropertyRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;  
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlobService _blobService;
        private readonly IAuthorizationService _authorizationService;
        public PropertyCategoryController(
            IPropertyCategoryRepository propertyCategoryRepository,
            IPropertyRepository propertyRepository,
            ICategoryRepository categoryRepository,
            IDocumentRepository documentRepository,
            IPropertyImageRepository propertyImageRepository,
            UserManager<ApplicationUser> userManager, 
            IBlobService blobService, 
            IAuthorizationService authorizationService,
            IUserPropertyRepository userPropertyRepository)
        {
            _propertyCategoryRepository = propertyCategoryRepository;
            _propertyRepository = propertyRepository;
            _userPropertyRepository = userPropertyRepository;
            _categoryRepository = categoryRepository;
            _documentRepository = documentRepository;
            _propertyImageRepository = propertyImageRepository;  
            _userManager = userManager;
            _blobService = blobService;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        private async Task<AuthorizationResult> CheckAuthorizationByPropertyId(int id)
        {
            // Create a requirement instance with the actual property ID
            var requirement = new UserPropertyManagerRequirement(id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, requirement);
            return authorizationResult;
        }

        private async Task<CategoryViewModel> CreateCategoryViewModel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var userproperty = await _userPropertyRepository.GetByPropertyIdAndUserId(id, userId);
            if (userproperty == null)
            {
                return null;
            }

            var imageList = new List<Dictionary<int, List<string>>>();
            var propertyImages = await _propertyImageRepository.GetByPropertyId(userproperty.PropertyId);

            if (propertyImages.Any())
            {
                var managerId = await _userPropertyRepository.GetManagerUserIdByPropertyId(userproperty.PropertyId);
                var containerName = "userid-" + managerId;
                var prefix = $"{userproperty.Property.PropertyName}/images/".Trim();
                var blobs = await _blobService.GetBlobUrisByPrefix(containerName, prefix);

                if (blobs != null && blobs.Any())
                {
                    foreach (var kvp in blobs)
                    {
                        var updatedList = new List<string>
                        {
                            kvp.Value[0], // Blob URL
                            kvp.Value[1], // Blob details
                            propertyImages.FirstOrDefault(i => i.FileName == kvp.Value[0])?.IsDisplay.ToString() 
                        };
                        imageList.Add(new Dictionary<int, List<string>> { { kvp.Key, updatedList } });
                    }
                }
            }

            var categories = await _propertyCategoryRepository.GetByPropertyId(id);
            List<CategoryPreviewViewModel> previewViewModels = new List<CategoryPreviewViewModel>();

            foreach (var category in categories)
            {
                var documentsByCategory = new Dictionary<int, List<Document>>();

                var documents = await _documentRepository.GetByPropertyCategoryId(category.PropertyCategoryId);
                var documentCount = 0;
                if (documents == null)
                {

                    previewViewModels.Add(new CategoryPreviewViewModel(category, null, documentCount, userproperty.Role.Name));
                    continue;
                }
                documentCount = documents.Count();
                documents = documents.Where(d => d.IsActiveNote == true).Take(2).ToList();
                documentsByCategory[category.PropertyCategoryId] = documents;
                previewViewModels.Add(new CategoryPreviewViewModel(category, documentsByCategory, documentCount, userproperty.Role.Name));
            }
            var viewModel = new CategoryViewModel(categories, imageList, userproperty.Property, previewViewModels, userproperty.Role.Name);
            return viewModel;
        }

        public async Task<IActionResult> Index(int id)
        {
            var viewModel = await CreateCategoryViewModel(id);
            if (viewModel == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }
            return View(viewModel);
        }




        [HttpGet]
        public async Task<IActionResult> UpdateCategoryCanvas(int id)
        {
            var viewModel = await CreateCategoryViewModel(id);
            if (viewModel == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }
            return PartialView("_CategoryCanvas", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CategoryFormPartial(int id)
        {
            var currentProperty = await _propertyRepository.GetById(id);
            if (currentProperty == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                }); 
            }
            var categories = await _categoryRepository.Categories();
            var categoryFormViewModel = new CategoryFormViewModel(categories, id, null);
            return PartialView("_AddCategoryForm", categoryFormViewModel);
        }

        
        [HttpPost]
        public async Task<IActionResult> AddCategory(int id, string name)
        {
            var currentProperty = await _propertyRepository.GetById(id);
            if (currentProperty == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "Could not find matching property"
                });
            }
            var authorizationResult = await CheckAuthorizationByPropertyId(id);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            if (name != null) 
            { 
                var categories = await _categoryRepository.Categories();
                var existing = categories.FirstOrDefault(c => c.CategoryName == name);
                if (existing != null)
                {
                    var newCategory = new PropertyCategory
                    {
                        PropertyId = id,
                        CategoryId = existing.CategoryId
                    };
                    await _propertyCategoryRepository.AddPropertyCategory(newCategory);
                    return Json(new { success = true });
                }
                else
                {
                    var newCategory = new PropertyCategory
                    {
                        PropertyId = id,
                        CustomCategory = name
                    };
                    await _propertyCategoryRepository.AddPropertyCategory(newCategory);
                    return Json(new { success = true });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred when adding new category. Please try again"
            });
        }

        public async Task<IActionResult> DeleteConfirmationPartial(int id)
        {
            var category = await _propertyCategoryRepository.GetById(id);
            if (category == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "Could not find matching category"
                });
            }
            return PartialView("_CategoryDeleteConfirmation", category);
        }

        
        [HttpPost]
        public async Task<IActionResult> UpdateColor(int id, string color)
        {
            var propertyCategory = await _propertyCategoryRepository.GetById(id);
            if (propertyCategory == null)
            {
                return NotFound();
            }
            var authorizationResult = await CheckAuthorizationByPropertyId(propertyCategory.Property.PropertyId);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            propertyCategory.Color = color;
            await _propertyCategoryRepository.UpdatePropertyCategory(propertyCategory);

            return Ok();
        }

        public async Task<IActionResult> EditCategoryFormPartial(int id, int currentCategoryId)
        {
            
            var currentProperty = await _propertyRepository.GetById(id);
            if (currentProperty == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }
            var currentCategory = await _propertyCategoryRepository.GetById(currentCategoryId);
            if (currentCategory == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "Could not find matching category"
                });
            }
            var categories = await _categoryRepository.Categories();
            var categoryFormViewModel = new CategoryFormViewModel(categories, id, currentCategory);
            return PartialView("_EditCategoryForm", categoryFormViewModel);
        }
    }
}

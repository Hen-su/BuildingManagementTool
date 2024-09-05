using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Controllers
{
    [Authorize]
    public class PropertyCategoryController : Controller
    {
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICategoryRepository _categoryRepository;
        public PropertyCategoryController(IPropertyCategoryRepository propertyCategoryRepository, IPropertyRepository propertyRepository, ICategoryRepository categoryRepository)
        {
            _propertyCategoryRepository = propertyCategoryRepository;
            _propertyRepository = propertyRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index(int id)
        {
            //test id
            id = 1;
            var property = await _propertyRepository.GetById(id);
            var categories = await _propertyCategoryRepository.GetByPropertyId(id);
            var img = "/imgs/sample-house.jpeg";
            var viewModel = new CategoryViewModel(categories, img, property);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategoryCanvas(int id)
        {
            //test id
            id = 1;
            var property = await _propertyRepository.GetById(id);
            var categories = await _propertyCategoryRepository.GetByPropertyId(id);
            var img = "/imgs/sample-house.jpeg";
            var viewModel = new CategoryViewModel(categories, img, property);
            return PartialView("_CategoryCanvas", viewModel);
        }

        //test id being used in call function
        [HttpGet]
        public async Task<IActionResult> CategoryFormPartial(int id)
        {
            var currentProperty = await _propertyRepository.GetById(id);
            if (currentProperty == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Property Not Found",
                    Detail = "The selected property was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            var categories = await _categoryRepository.Categories();
            var categoryFormViewModel = new CategoryFormViewModel(categories, id);
            return PartialView("_CategoryForm", categoryFormViewModel);
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
    }
}

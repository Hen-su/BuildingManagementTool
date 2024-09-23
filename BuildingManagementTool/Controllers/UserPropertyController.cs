using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Controllers
{
    public class UserPropertyController : Controller
    {
        private readonly IUserPropertyRepository _userPropertyRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserPropertyController(IUserPropertyRepository userPropertyRepository, IPropertyRepository propertyRepository, UserManager<ApplicationUser> userManager)
        {
            _userPropertyRepository = userPropertyRepository;
            _propertyRepository = propertyRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null) 
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            var userId = user.Id;
            var propertyList = await _userPropertyRepository.GetByUserId(userId);
            return View(propertyList);
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
            
            if (user == null)
            {
                return BadRequest("A problem occurred while retrieving your data");
            }
            var userId = user.Id;
            var propertyList = await _userPropertyRepository.GetByUserId(userId);
            return PartialView("_PropertyContainer", propertyList);
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
    }
}

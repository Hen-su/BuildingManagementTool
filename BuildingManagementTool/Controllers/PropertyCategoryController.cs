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
        public PropertyCategoryController(IPropertyCategoryRepository propertyCategoryRepository)
        {
            _propertyCategoryRepository = propertyCategoryRepository;
        }
        public async Task<IActionResult> Index(int id)
        {
            //test id
            id = 1;
            var categories = await _propertyCategoryRepository.GetByPropertyId(id);
            var img = "/imgs/sample-house.jpeg";
            var viewModel = new CategoryViewModel(categories, img);
            return View(viewModel);
        }
    }
}

using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Controllers
{
    public class CategoryController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
    }
}

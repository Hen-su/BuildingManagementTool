using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Controllers
{
    public class FilesController : Controller
    {
        private readonly IFileRepository _fileRepository;
        public FilesController(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadFormPartial()
        {
            return PartialView("_UploadForm");
        }
    }
}

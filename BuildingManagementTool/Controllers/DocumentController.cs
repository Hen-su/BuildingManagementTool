using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _fileRepository;
        public DocumentController(IDocumentRepository fileRepository)
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
        public IActionResult DocumentCardPartial()
        {
            return PartialView("_DocumentCard");
        }
    }
}

using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO.Pipelines;

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
            var documents = _fileRepository.AllDocuments;
            return View(documents);
        }

        public IActionResult UploadFormPartial()
        {
            return PartialView("_UploadForm");
        }
     


    }
}

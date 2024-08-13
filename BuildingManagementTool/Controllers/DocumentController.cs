﻿using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BuildingManagementTool.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _fileRepository;

        public DocumentController(IDocumentRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public IActionResult Index(int? selectedDocumentId)
        {
            var documents = _fileRepository.AllDocuments;
            Document selectedDocument = null; //no document selected 

            // only selection of documents on clicks
            if (selectedDocumentId.HasValue)
            {// if selectedDocumentId is not null it will match the documentId's of all documents to the selected one and pass it to selectedDocument
                selectedDocument = documents.FirstOrDefault(d => d.DocumentId == selectedDocumentId.Value);
            }

            // Pass documents and selectedDocument to the view 
            return View((documents, selectedDocument));
        }


        public IActionResult UploadFormPartial()
        {
            return PartialView("_UploadForm");
        }
    }
}

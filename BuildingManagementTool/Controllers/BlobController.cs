using Azure.Storage.Blobs;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Azure;

namespace BuildingManagementTool.Controllers
{
    public class BlobController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IBlobService _blobService;
        public BlobController(IBlobService blobService, IDocumentRepository documentRepository)
        {
            _blobService = blobService;
            _documentRepository = documentRepository;
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IList<IFormFile> files)
        {
            var containerName = "test1";
            if (files == null || files.Count == 0)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "File Upload Error",
                    Detail = "No file was uploaded. Please select a file before submitting.",
                    Status = StatusCodes.Status400BadRequest
                };
                return BadRequest(problemDetails);
            }

            foreach (var file in files)
            {
                string blobName = $"{containerName}/{file.FileName}";
                bool blobExists = await _blobService.BlobExistsAsync(containerName, blobName);
                if (blobExists)
                {
                    return Conflict("File already exists in blob storage.");
                }

                using (var stream = file.OpenReadStream())
                {
                    bool isUploaded = await _blobService.UploadBlobAsync(containerName, blobName, stream);
                    if (!isUploaded)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Title = "Upload Error",
                            Detail = "Failed to upload the file to blob storage",
                            Status = StatusCodes.Status500InternalServerError
                        };
                        return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
                    }

                }
                /*Switch Case for the image url using the content type*/

                var metadata = new Models.Document
                {
                    FileName = file.FileName,
                    BlobName = blobName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.UtcNow
                    

                };
                try
                {
                    await _documentRepository.AddDocumentData(metadata);
                }
                catch (Exception ex)
                {
                    await _blobService.DeleteBlobAsync(containerName, blobName);

                    var problemDetails = new ProblemDetails
                    {
                        Title = "Database Error",
                        Detail = "Failed to save metadata in database. The uploaded blob will be removed.",
                        Status = StatusCodes.Status500InternalServerError
                    };
                    return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
                }
            }
            //TempData["response"] = "Upload Successful";
            return RedirectToAction("Index", "Document");
        }
        [HttpGet]
        public async Task<IActionResult> ListBlobs()
        {
            var containerName = "test1";
            var blobNames = await _blobService.ListBlobsAsync(containerName);

            if (blobNames == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to retrieve blobs.");
            }

            var documents = blobNames.Select(blobName => new BuildingManagementTool.Models.Document
            {
                FileName = Path.GetFileName(blobName), 
                BlobName = blobName
            }).ToList();

            return PartialView("_DocumentCardPartial", documents);
        }


    }
}

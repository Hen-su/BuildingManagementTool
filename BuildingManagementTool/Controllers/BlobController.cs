using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
            //Check submitted file list isn't empty
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
            //Check if a blob with the same name exists
            foreach (var file in files)
            {
                string blobName = $"{containerName}/{file.FileName}";
                bool blobExists = await _blobService.BlobExistsAsync(containerName, blobName);
                if (blobExists)
                {
                    return Conflict($"A blob with the same name already exists. {blobName}");
                }
                //Upload to blob storage
                using (var stream = file.OpenReadStream())
                {
                    BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType };
                    bool isUploaded = await _blobService.UploadBlobAsync(containerName, blobName, stream, blobHttpHeaders);
                    //Check if upload was successful
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

                //Create metadata
                var metadata = new Models.Document
                {
                    FileName = file.FileName,
                    BlobName = blobName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.UtcNow
                    

                };
                //Add record to SQL Server
                try
                {
                    await _documentRepository.AddDocumentData(metadata);
                }
                //Remove blob if metadata was not added
                catch
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
            TempData["response"] = "Upload Successful";
            return RedirectToAction("Index", "Document");
        }

        public async Task<IActionResult> DeleteBlob(int? id)
        {
            //test container
            var containerName = "test1";
            //Check metadata exists
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Metadata Not Found",
                    Detail = "The File MetaData was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            //Delete from blob storage
            bool isDeleted = await _blobService.DeleteBlobAsync(containerName, document.BlobName);
            //Check if succcessful
            if (!isDeleted)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Deletion Error",
                    Detail = "An error occurred when deleting the file",
                    Status = StatusCodes.Status500InternalServerError
                };
                return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
            }
            //Delete metadata if blob is deleted
            await _documentRepository.DeleteDocumentData(document);
            return RedirectToAction("Index", "Document");
        }

        public async Task<IActionResult> DeleteConfirmationPartial(int id)
        {
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Metadata Not Found",
                    Detail = "The File MetaData was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            return PartialView("_DeleteConfirmation", document);
        }

        public IActionResult PDFViewerPartial(string blobUrl)
        {
            return PartialView("_PDFViewer", blobUrl);
        }

        [HttpGet] 
        public async Task<IActionResult> Download(int id)
        {
            //test container
            var containerName = "test1";
            //Check metadata exists
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Document Not Found",
                    Detail = "The Document was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            //Download blob
            var stream = await _blobService.DownloadBlobAsync(containerName, document.BlobName);
            if (stream == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "File Not Found",
                    Detail = "The file was not found in blob storage",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }

            return File(stream, document.ContentType, document.FileName);
        }
        //Renders Partial Views to Modal container
        public IActionResult VideoPlayerPartial(string blobUrl)
        {
            return PartialView("_VideoPlayer", blobUrl);
        }

        public IActionResult ImageViewerPartial(string blobUrl)
        {
            return PartialView("_ImageViewer", blobUrl);
        }
        //Gets Url of blobs and handles rendering based on content type
        public async Task<IActionResult> RenderFile(int id)
        {
            var containerName = "test1";
            //Check if metadata exists
            var document = await _documentRepository.GetById(id);
            if (document == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Metadata Not Found",
                    Detail = "The File MetaData was not found",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            var blobName = document.BlobName;
            //Get URL of blob by name and container
            var blobUrl = await _blobService.GetBlobUrlAsync(containerName, blobName);
            if (blobUrl == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "File Not Found",
                    Detail = "The file was not found in blob storage",
                    Status = StatusCodes.Status404NotFound
                };
                return StatusCode(StatusCodes.Status404NotFound, problemDetails);
            }
            //Handle rendering of blob by MIME content type
            if (document.ContentType == "application/pdf")
            {
                return PDFViewerPartial(blobUrl);
            }
            else if (document.ContentType.StartsWith("video"))
            {
                return VideoPlayerPartial(blobUrl);
            }
            else if (document.ContentType.StartsWith("image"))
            {
                return ImageViewerPartial(blobUrl);
            }
            else
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Unsupported File Type",
                    Detail = "The content type is not supported for rendering",
                    Status = StatusCodes.Status415UnsupportedMediaType
                };
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, problemDetails);
            }
        }
    }
}

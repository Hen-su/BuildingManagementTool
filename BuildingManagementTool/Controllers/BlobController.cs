using Azure.Storage.Blobs;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

namespace BuildingManagementTool.Controllers
{
    public class BlobController : Controller
    {
        private readonly IFileRepository _fileRepository;
        private readonly BlobService _blobService;
        public BlobController(BlobService blobService, IFileRepository fileRepository)
        {
            _blobService = blobService;
            _fileRepository = fileRepository;
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IFormFile file)
        {
            var containerName = "test1";
            string blobName = $"{containerName}/{file.FileName}";
            if (file == null || file.Length == 0)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "File Upload Error",
                    Detail = "No file was uploaded. Please select a file before submitting.",
                    Status = StatusCodes.Status400BadRequest
                };
                return BadRequest(problemDetails);
            }

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

            var metadata = new Models.File
            {
                FileName = file.FileName,
                BlobName = blobName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadDate = DateTime.UtcNow
            };
            try
            {
                await _fileRepository.AddFileData(metadata);
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
            return Ok(new
            {
                Message = "File uploaded successfully."
            });
        }
    }
}

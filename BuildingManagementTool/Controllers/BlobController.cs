using Azure.Storage.Blobs;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Controllers
{
    public class BlobController : Controller
    {
        private readonly BlobService _blobService;
        public BlobController(BlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(string fileName, IFormFile file)
        {
            var containerName = "test";
            string blobName = $"{containerName}/{fileName}";
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

            using (var stream = file.OpenReadStream())
            {
                await _blobService.UploadBlobAsync(containerName, blobName, stream);
            }

            return Ok(new
            {
                Message = "File uploaded successfully."
            });
        }
    }
}

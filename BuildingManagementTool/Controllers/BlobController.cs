﻿using Azure.Storage.Blobs;
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
                    BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType };
                    bool isUploaded = await _blobService.UploadBlobAsync(containerName, blobName, stream, blobHttpHeaders);
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
            //testid
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
            bool isDeleted = await _blobService.DeleteBlobAsync(containerName, document.BlobName);
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
            await _documentRepository.DeleteDocumentData(document);
            return RedirectToAction("Index", "Document");
        }

        public async Task<IActionResult> PDFViewerPartial(string blobUrl)
        {
            return PartialView("_PDFViewer", blobUrl);
        }

        [HttpGet] 
        public async Task<IActionResult> Download(int id)
        {
            //test container
            var containerName = "test1";

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
     
        public async Task<IActionResult> VideoPlayerPartial(string blobUrl)
        {
            return PartialView("_VideoPlayer", blobUrl);
        }

        public async Task<IActionResult> RenderFile(int id)
        {
            var containerName = "test1";
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
            
            if (document.ContentType == "application/pdf")
            {
                return await PDFViewerPartial(blobUrl);
            }
            else if (document.ContentType.StartsWith("video"))
            {
                return await VideoPlayerPartial(blobUrl);
            }/*
            else if (document.ContentType.StartsWith("image"))
            {
                return await ImageViewerPartial(blobUrl);
            }*/
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

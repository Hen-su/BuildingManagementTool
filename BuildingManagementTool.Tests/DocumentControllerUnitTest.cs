using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Moq;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Document = BuildingManagementTool.Models.Document;


namespace BuildingManagementTool.Tests
{
    internal class DocumentControllerUnitTest
    {
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private DocumentController _documentController;

        [SetUp]
        public void Setup()
        {
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _documentController = new DocumentController(_mockDocumentRepository.Object, _mockPropertyCategoryRepository.Object);
        }

        [Test]
        public async Task Index_PropertyCategoryExists_ReturnsPartialView()
        {
            int id = 1;
            PropertyCategory propertyCategory = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };
            _mockPropertyCategoryRepository.Setup(p => p.GetById((id))).ReturnsAsync(propertyCategory);

            var documents = new List<Document>
            {
                new Document { DocumentId = 1, FileName = "Document 1", PropertyCategoryId = 1 },
                new Document { DocumentId = 2, FileName = "Document 2", PropertyCategoryId = 2 }
            };
            documents = documents.Where(p => p.PropertyCategoryId == propertyCategory.PropertyId).ToList();

            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(documents);

            var result = await _documentController.Index(1) as PartialViewResult;

            Assert.IsNotNull(result);
            var partialViewResult = (PartialViewResult)result;
            Assert.That(partialViewResult.ViewName.Equals("_DocumentIndex"));

            var viewModel = partialViewResult.Model as DocumentViewModel;
            Assert.IsNotNull(viewModel);
            Assert.That(viewModel.Documents.Count().Equals(1));
            Assert.IsTrue(viewModel.Documents.All(d => d.PropertyCategoryId == 1));
        }

        [Test]
        public async Task Index_PropertyCategoryNotExists_()
        {
            int id = 1;
            PropertyCategory propertyCategory = null; 
            _mockPropertyCategoryRepository.Setup(p => p.GetById((id))).ReturnsAsync(propertyCategory);

            var result = await _documentController.Index(id) as Object;

            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Title.Equals("Category Not Found"));
            Assert.That(problemDetails.Detail.Equals("The selected category was not found"));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
        }

        [Test]
        public void GetDocumentPreview_DocumentFound_Success()
        {
            var documentId = 1;
            var document = new List<Document> { new Document { DocumentId = 1, FileName = "Document 1", PropertyCategoryId = 1 } };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
               .Returns(document);

            var result = _documentController.GetDocumentOptions(documentId) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentOptions", result.ViewName);
            Assert.AreEqual(document[0], result.Model);
        }

         
        [Test]
        public void GetDocumentPreview_DocumentFound_Fail()
        {
            var documentId = 1;
            var result = _documentController.GetDocumentOptions(documentId) as Object;
            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.Multiple(() =>
            {
                Assert.That(problemDetails.Title.Equals("Metadata Not Found"));
                Assert.That(problemDetails.Detail.Equals("The File MetaData was not found"));
                Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
            });
        }

        [Test]
        public async Task UpdateList_Success()
        {
            int id = 1;
            PropertyCategory propertyCategory = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };
            _mockPropertyCategoryRepository.Setup(p => p.GetById(It.IsAny<int>())).ReturnsAsync(propertyCategory);

            var documents = new List<Document>
            {
                new Document { DocumentId = 1, FileName = "Document 1", PropertyCategoryId = 1 },
                new Document { DocumentId = 2, FileName = "Document 2", PropertyCategoryId = 2 }
            };
            documents = documents.Where(p => p.PropertyCategoryId == propertyCategory.PropertyId).ToList();

            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(documents);

            var result = await _documentController.UpdateList(1) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentList", result.ViewName);
        }

        [Test]
        public async Task UpdateList_CategoryNotExists_Fail()
        {
            int id = 1;

            var result = await _documentController.UpdateList(1) as Object;

            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Title.Equals("Category Not Found"));
            Assert.That(problemDetails.Detail.Equals("The selected category was not found"));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task UploadForm_CategoryExists_Success()
        {
            int id = 1;
            PropertyCategory propertyCategory = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };
            _mockPropertyCategoryRepository.Setup(p => p.GetById(It.IsAny<int>())).ReturnsAsync(propertyCategory);

            var result = await _documentController.UploadFormPartial(id) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_UploadForm", result.ViewName);
        }

        [Test]
        public async Task UploadForm_CategoryNotExists_Fail()
        {
            int id = 1;

            var result = await _documentController.UploadFormPartial(1) as Object;

            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Title.Equals("Category Not Found"));
            Assert.That(problemDetails.Detail.Equals("The selected category was not found"));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
        }









        [Test]
        public async Task AddNoteFormPartial_DocumentExists_Success()
        {
            int id = 1;
            Document document = new Document { DocumentId = 1};
            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());

            var result = await _documentController.AddNoteFormPartial(id) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_AddNoteForm", result.ViewName);
            Assert.AreEqual(document, result.Model);
        }





        [Test]
        public async Task AddNoteFormPartial_DocumentNotExists_Fail()
        {
            int id = 1;

            var result = await _documentController.AddNoteFormPartial(1) as Object;

            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Title.Equals("Document Not Found"));
            Assert.That(problemDetails.Detail.Equals("The document was not found."));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));

        }




        /*

        [Test]
        public async Task AddNoteToDocument_DocumentExists_Sucess()
        {
           int id = 1;
            Document document = new Document { DocumentId = 1};
            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());
            var result = await _documentController.AddNoteToDocument(1, "testnote") as NotFoundObjectResult;

            Assert.IsNotNull(result);
            var responseObject = result.Value as dynamic;
            Assert.AreEqual(true, responseObject.success);
            Assert.AreEqual("Note added successfully!", responseObject.message);

        }



        [Test]
        public async Task AddNoteToDocument_DocumentNotExists_Fail()
        {
            int id = 1;
            var result = await _documentController.AddNoteToDocument(1, "testnote") as NotFoundObjectResult;

            Assert.IsNotNull(result);
            var responseObject = result.Value as dynamic;
            Assert.AreEqual(false, responseObject.success);
            Assert.AreEqual("Document not found.", responseObject.message);

        }

        */


        [Test]
        public async Task GetDocumentNotesByCategory_CategoryNotFound_Fail()
        {
            
            int categoryId = 1;
            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(categoryId)).ReturnsAsync((PropertyCategory)null);

          
            var result = await _documentController.GetDocumentNotesByCategory(categoryId) as ObjectResult;

         
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);

            var problemDetails = result.Value as ProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual("Category Not Found", problemDetails.Title);
            Assert.AreEqual("The selected property category was not found.", problemDetails.Detail);
        }
      
        
        /*
        [Test]
        public async Task GetDocumentNotesByCategory_DocumentNotFound_Fail()
        {
        
            int categoryId = 1;
            var category = new PropertyCategory { PropertyCategoryId = categoryId };

            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(categoryId)).ReturnsAsync(category);
            _mockDocumentRepository.Setup(d => d.AllDocuments)
                                   .Returns(new List<Document>().AsQueryable()); // No documents for this category

         
            var result = await _documentController.GetDocumentNotesByCategory(categoryId);

            Assert.IsNotNull(result); // Ensure the result is not null

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult); // Check if the result is NotFoundObjectResult
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var responseObject = notFoundResult.Value as IDictionary<string, object>;
        
            Assert.AreEqual(false, responseObject["success"]);
            Assert.AreEqual("No documents found for this property category.", responseObject["message"]);
        }

        */




        [Test]
        public async Task GetDocumentNotesByCategory_DocumentsFound_Success()
        {
          
            int categoryId = 1;
            var category = new PropertyCategory { PropertyCategoryId = categoryId };

            var documents = new List<Document>
        {
            new Document { DocumentId = 1, PropertyCategoryId = categoryId, Note = "Note 1" },
            new Document { DocumentId = 2, PropertyCategoryId = categoryId, Note = "Note 2" }
        };

            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(categoryId)).ReturnsAsync(category);
            _mockDocumentRepository.Setup(repo => repo.AllDocuments).Returns(documents.AsQueryable());

            var result = await _documentController.GetDocumentNotesByCategory(categoryId) as PartialViewResult;

            
            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentNotes", result.ViewName);

            var model = result.Model as IEnumerable<Document>;
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count());
        }

        [Test]
        public async Task NoteDeleteConfFormPartial_DocumentNotExists_Fail()
        {
            int id = 1;

            var result = await _documentController.NoteDeleteConfFormPartial(1) as Object;

            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Title.Equals("Document Not Found"));
            Assert.That(problemDetails.Detail.Equals("The document was not found."));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
        }


        [Test]
        public async Task NoteDeleteConfFormPartial_DocumentExists_Success()
        {
            int id = 1;
            Document document = new Document { DocumentId = 1 };

            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());

            var result = await _documentController.NoteDeleteConfFormPartial(id) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_NoteDeleteConfForm", result.ViewName);
            Assert.AreEqual(document, result.Model);
        }
        [Test]
        public async Task DeleteNoteFromDocument_DocumentNotFound_Fail()
        {
         
            int documentId = 1;
            _mockDocumentRepository.Setup(repo => repo.AllDocuments).Returns(new List<Document>().AsQueryable());

            
            var result = await _documentController.DeleteNoteFromDocument(documentId) as JsonResult;

     
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);

            var responseObject = result.Value as IDictionary<string, object>;
            Assert.IsNotNull(responseObject);
            Assert.AreEqual(false, responseObject["success"]);
            Assert.AreEqual("Document not found.", responseObject["message"]);
        }



        [Test]
        public async Task DeleteNoteFromDocument_InActiveNoteDocument_Success()
        {
          
            int documentId = 1;
            var document = new Document { DocumentId = documentId, IsActiveNote = false, Note = "Some note" };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments).Returns(new List<Document> { document }.AsQueryable());
            _mockDocumentRepository.Setup(repo => repo.UpdateDocumentAsync(document)).Returns(Task.CompletedTask);

            var result = await _documentController.DeleteNoteFromDocument(documentId) as JsonResult;

      
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));
            Assert.AreEqual("Note deleted successfully!", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));
        }


        [Test]
        public async Task DeleteNoteFromDocument_ActiveDocument_Fail()
        {
          
            int documentId = 1;
            var document = new Document { DocumentId = documentId, IsActiveNote = true, Note = "Some note" };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments).Returns(new List<Document> { document }.AsQueryable());

        
            var result = await _documentController.DeleteNoteFromDocument(documentId) as BadRequestObjectResult;

            
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    
            var responseObject = result.Value as IDictionary<string, object>;
            Assert.IsNotNull(responseObject);
            Assert.AreEqual(false, responseObject["success"]);
            Assert.AreEqual("Cannot delete an active note.", responseObject["message"]);
        }



        [Test]
        public async Task SetActiveNote_DocumentNotFound_Fail()
        {
           
            int documentId = 1;
            bool isActive = true;

            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
                .Returns(new List<Document>().AsQueryable());

           
            var result = await _documentController.SetActiveNote(documentId, isActive) as JsonResult;

            
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));
            Assert.AreEqual("Document not found.", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));
        }




        [Test]
        public async Task SetActiveNote_Limit_Fail()
        {
          
            int documentId = 1;
            bool isActive = true;

            var documents = new List<Document>
    {
        new Document { DocumentId = 1, IsActiveNote = true },
        new Document { DocumentId = 2, IsActiveNote = true },
        new Document { DocumentId = 3, IsActiveNote = false }
    }; 
            
            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
        .Returns(documents.AsQueryable());


       
            var result = await _documentController.SetActiveNote(3, isActive) as JsonResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));
            Assert.AreEqual("You can only have 2 active notes.", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));

        }


        [Test]
        public async Task SetActiveNote_Limit_Success()
        {
            // Arrange
            int documentId = 1;
            bool isActive = true;

            var documents = new List<Document>
    {
        new Document { DocumentId = 1, IsActiveNote = false },
        new Document { DocumentId = 2, IsActiveNote = false },
     };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
        .Returns(documents.AsQueryable());
            _mockDocumentRepository.Setup(repo => repo.UpdateDocumentAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);


            // Act
            var result = await _documentController.SetActiveNote(documentId, isActive) as JsonResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));
            Assert.AreEqual("Active note status updated!", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));

        }


        [Test]
        public async Task DocumentRenameFormPartial_DocumentNotExists_Fail()
        {
            int id = 1;

            var result = await _documentController.DocumentRenameFormPartial(1) as Object;

            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Title.Equals("Document Not Found"));
            Assert.That(problemDetails.Detail.Equals("The document was not found."));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
        }



        [Test]
        public async Task DocumentRenameFormPartial_DocumentExists_Success()
        {
            int id = 1;
            Document document = new Document { DocumentId = 1 };

            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());

            var result = await _documentController.DocumentRenameFormPartial(id) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentRenameForm", result.ViewName);
            Assert.AreEqual(document, result.Model);
        }



        [Test]
        public async Task DocumentFileNameRename_DocumentNotFound_Fail()
        {
            var documentId = 1;
            var filename = "newname.txt";

            // Mock the repository to return an empty collection
            _mockDocumentRepository.Setup(d => d.AllDocuments)
                .Returns(new List<Document>().AsQueryable());

            var result = await _documentController.DocumentFileNameRename(documentId, filename);

            Assert.NotNull(result);  
            var objectResult = result as ObjectResult; 
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, objectResult.StatusCode); 
        }


        [Test]
        public async Task DocumentFileNameRename_DocumentExists_Success()
        {
           
            var documentId = 1;
            var oldFilename = "oldname.txt";  
            var newFilename = "newname.txt";  
            var document = new Document { DocumentId = documentId, FileName = oldFilename };

            _mockDocumentRepository.Setup(d => d.AllDocuments)
                .Returns(new List<Document> { document }.AsQueryable());

           
            var result = await _documentController.DocumentFileNameRename(documentId, newFilename) as JsonResult;

            Assert.IsNotNull(result); 
            Assert.AreEqual(true, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));  
            Assert.AreEqual("Document renamed successfully!", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));  

          
            Assert.AreEqual(newFilename, document.FileName);
        }


        [TearDown]
        public void Teardown()
        {
            _documentController.Dispose();
        }

    }


}

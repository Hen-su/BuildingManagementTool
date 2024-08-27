using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Moq;
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


        [TearDown]
        public void Teardown()
        {
            _documentController.Dispose();
        }

    }


}

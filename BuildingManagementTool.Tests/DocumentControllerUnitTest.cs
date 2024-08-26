using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BuildingManagementTool.Tests
{
    internal class DocumentControllerUnitTest
    {
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private DocumentController _documentController;

        [SetUp]
        public void Setup()
        {
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _documentController = new DocumentController(_mockDocumentRepository.Object);
        }
        [Test]
        public void Index_SelectedDocument_ReturnsViewWithSelectedDocument()
        {
            var documents = new List<Document>
            {
                new Document { DocumentId = 1, FileName = "Document 1" },
                new Document { DocumentId = 2, FileName = "Document 2" }
            };
            _mockDocumentRepository.Setup(repo => repo.AllDocuments).Returns(documents);

            
            var result = _documentController.Index(1) as ViewResult;

           
            Assert.IsNotNull(result);

            if (result.Model is (IEnumerable<Document> documentList, Document selectedDocument))
            {
                Assert.AreEqual(documents, documentList);// list should match
                Assert.AreEqual(documents.First(), selectedDocument); //First document should be selected
            }
            else
            {
                Assert.Fail("The model was not of the expected tuple type.");
            }
        }


        [Test]
        public void Index_NoSelectedDocument_ReturnsViewWithoutSelectedDocument()
        {
            var documents = new List<Document>
        {
            new Document { DocumentId = 1, FileName = "Document 1" },
            new Document { DocumentId = 2, FileName = "Document 2" }
        };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments).Returns(documents);

            var result = _documentController.Index(null) as ViewResult; // Passing null for no document selected

            Assert.IsNotNull(result);

            var model = result.Model as ValueTuple<IEnumerable<Document>, Document>?;

            Assert.IsTrue(model.HasValue, "The model was not of the expected tuple type.");

            var (documentList, selectedDocument) = model.Value;

            Assert.AreEqual(documents, documentList); // Document list should match
            Assert.IsNull(selectedDocument); // No document should be selected
        }



        [Test]
        public void GetDocumentPreview_DocumentFound_Success()
        {

            var documentId = 1;
            var document = new Document { DocumentId = documentId, FileName = "Document 1" };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
               .Returns(new List<Document> { document });

           
            var result = _documentController.GetDocumentPreview(documentId) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentPreview", result.ViewName);
            Assert.AreEqual(document, result.Model);

        }


        [Test]
        public void GetDocumentPreview_DocumentFound_Fail()
        {
            var documentId = 1;
            var document = new Document { DocumentId = documentId, FileName = "Document 1" };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
               .Returns(new List<Document> { document });

          
            var result = _documentController.GetDocumentPreview(documentId) as PartialViewResult;

            if(result == null)
            {
                Assert.Fail("View not found");
            }

        }

        [TearDown]
        public void Teardown()
        {
            _documentController.Dispose();
        }

    }


}

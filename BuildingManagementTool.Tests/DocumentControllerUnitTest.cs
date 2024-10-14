using BuildingManagementTool.Controllers;
using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Moq;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Document = BuildingManagementTool.Models.Document;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Policy;




namespace BuildingManagementTool.Tests
{
    internal class DocumentControllerUnitTest
    {
        private Mock<IDocumentRepository> _mockDocumentRepository;
        private Mock<IPropertyCategoryRepository> _mockPropertyCategoryRepository;
        private DocumentController _documentController;
        private Mock<IRazorViewToStringRenderer> _renderer; 
        private Mock<IEmailSender> _mockEmailSender;

        private Mock<IUserPropertyRepository> _mockUserPropertyRepository;
        private Mock<IUserStore<ApplicationUser>> _mockUserStore;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IRoleStore<IdentityRole>> _mockRoleStore;
        private Mock<IAuthorizationService> _mockAuthorizationService;

        [SetUp]
        public void Setup()
        {
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockPropertyCategoryRepository = new Mock<IPropertyCategoryRepository>();
            _mockEmailSender = new Mock<IEmailSender>();
            _renderer = new Mock<IRazorViewToStringRenderer>();

            _mockUserPropertyRepository = new Mock<IUserPropertyRepository>();
            _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(_mockUserStore.Object, null, null, null, null, null, null, null, null);
            _mockAuthorizationService = new Mock<IAuthorizationService>();

            _documentController = new DocumentController(
                          _mockDocumentRepository.Object,
                          _mockPropertyCategoryRepository.Object,
                          _mockEmailSender.Object,
                          _renderer.Object,
                          _mockUserManager.Object,
                          _mockUserPropertyRepository.Object,
                          _mockAuthorizationService.Object
                      );
        }

        [Test]
        public async Task Index_PropertyCategoryExists_ReturnsPartialView()
        {
            int id = 1;
            PropertyCategory propertyCategory = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = role.Id, Role = role };
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockUserPropertyRepository.Setup(x => x.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);
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
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, NormalizedName = "Manager" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = role.Id, Role = role };
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _mockUserPropertyRepository.Setup(x => x.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);
            _mockPropertyCategoryRepository.Setup(p => p.GetById((id))).ReturnsAsync(propertyCategory);

            var result = await _documentController.Index(id) as Object;

            var objectResult = (ObjectResult)result;
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.That(problemDetails.Title.Equals("Category Not Found"));
            Assert.That(problemDetails.Detail.Equals("The selected category was not found"));
            Assert.That(problemDetails.Status.Equals(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task GetDocumentOptions_DocumentFound_Success()
        {
            var documentId = 1;
            var propertyCategory = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };
            var document = new Document { DocumentId = 1, FileName = "Document 1", PropertyCategoryId = 1, PropertyCategory = propertyCategory };
            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, Name = "Manager" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, UserId = userId, RoleId = role.Id, Role = role };
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserPropertyRepository.Setup(x => x.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);
            _mockDocumentRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
               .ReturnsAsync(document);

            var result = await _documentController.GetDocumentOptions(documentId) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentOptions", result.ViewName);
        }

         
        [Test]
        public async Task GetDocumentOptions_DocumentFound_Fail()
        {
            var documentId = 1;
            var result = await _documentController.GetDocumentOptions(documentId);
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
            PropertyCategory propertyCategory = new PropertyCategory { PropertyId = 1 };
            Document document = new Document { DocumentId = 1, PropertyCategory = propertyCategory};
            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _documentController.AddNoteFormPartial(id) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_AddNoteForm", result.ViewName);
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

        [Test]
        public async Task AddNoteToDocument_DocumentExists_Sucess()
        {
            int id = 1;
            PropertyCategory propertyCategory = new PropertyCategory { PropertyId = 1 };
            Document document = new Document { DocumentId = 1, PropertyCategory = propertyCategory };
            var viewModel = new AddNoteViewModel { Note = "testnote" };
            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());
            _mockDocumentRepository.Setup(d => d.UpdateDocumentAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _documentController.AddNoteToDocument(1, viewModel);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True, message = Note added successfully! }"));
        }



        [Test]
        public async Task AddNoteToDocument_DocumentNotExists_Fail()
        {
            int id = 1;
            var viewModel = new AddNoteViewModel { Note = "testnote" };
            var result = await _documentController.AddNoteToDocument(1, viewModel) as NotFoundObjectResult;

            Assert.IsNotNull(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound));
        }


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
      
        
        [Test]
        public async Task GetDocumentNotesByCategory_DocumentNotFound_Fail()
        {
            int categoryId = 1;
            var category = new PropertyCategory { PropertyCategoryId = categoryId };

            _mockPropertyCategoryRepository.Setup(repo => repo.GetById(categoryId)).ReturnsAsync(category);
            _mockDocumentRepository.Setup(d => d.GetByPropertyCategoryId(It.IsAny<int>()))
                                   .ReturnsAsync(new List<Document>()); // No documents for this category
            
            var result = await _documentController.GetDocumentNotesByCategory(categoryId);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound));
        }


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
            _mockDocumentRepository.Setup(repo => repo.GetByPropertyCategoryId(It.IsAny<int>())).ReturnsAsync(documents);

            var userId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid().ToString();
            var email = "example@example.com";
            var user = new ApplicationUser() { Id = userId, Email = email, UserName = email };
            var role = new IdentityRole() { Id = roleId, Name = "Manager" };
            var property = new Property { PropertyId = 1, PropertyName = "Test Property" };
            var userproperty = new UserProperty { UserPropertyId = 1, PropertyId = 1, Property = property, UserId = userId, RoleId = role.Id, Role = role };
            _mockUserStore.Setup(us => us.FindByIdAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserPropertyRepository.Setup(u => u.GetByPropertyIdAndUserId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(userproperty);

            var result = await _documentController.GetDocumentNotesByCategory(categoryId) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentNotes", result.ViewName);

            var model = result.Model as DocumentNotesViewModel;
            Assert.IsNotNull(model);
            Assert.That(model.Documents.Count(), Is.EqualTo(2));
            Assert.That(model.Role, Is.EqualTo("Manager"));
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
            var category = new PropertyCategory { PropertyCategoryId = 1 };
            Document document = new Document { DocumentId = 1, PropertyCategory = category };

            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

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


            var result = await _documentController.DeleteNoteFromDocument(documentId);

     
            Assert.IsNotNull(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.That(objectResult.StatusCode.Equals(StatusCodes.Status404NotFound));
        }



        [Test]
        public async Task DeleteNoteFromDocument_InActiveNoteDocument_Success()
        {
            int documentId = 1;
            PropertyCategory propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document { DocumentId = documentId, IsActiveNote = false, Note = "Some note", PropertyCategory = propertyCategory };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments).Returns(new List<Document> { document }.AsQueryable());
            _mockDocumentRepository.Setup(repo => repo.UpdateDocumentAsync(document)).Returns(Task.CompletedTask);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _documentController.DeleteNoteFromDocument(documentId) as JsonResult;

      
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));
            Assert.AreEqual("Note deleted successfully!", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));
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

            PropertyCategory propertyCategory = new PropertyCategory { PropertyId = 1 };
            var documents = new List<Document>
            {
                new Document { DocumentId = 1, IsActiveNote = true, PropertyCategoryId = 1, PropertyCategory = propertyCategory },
                new Document { DocumentId = 2, IsActiveNote = true, PropertyCategoryId = 1, PropertyCategory = propertyCategory },
                new Document { DocumentId = 3, IsActiveNote = false, PropertyCategoryId = 1, PropertyCategory = propertyCategory }
            };

            var category = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
        .Returns(documents.AsQueryable());

            _mockPropertyCategoryRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(category);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _documentController.SetActiveNote(3, isActive) as JsonResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));
            Assert.AreEqual("Exceeded active notes limit.", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));

        }


        [Test]
        public async Task SetActiveNote_Limit_Success()
        {
            // Arrange
            int documentId = 1;
            bool isActive = true;
            var category = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };
            var documents = new List<Document>
            {
                new Document { DocumentId = 1, IsActiveNote = false, PropertyCategoryId = 1, PropertyCategory = category },
                new Document { DocumentId = 2, IsActiveNote = false, PropertyCategoryId = 1, PropertyCategory = category }
            };

            _mockDocumentRepository.Setup(repo => repo.AllDocuments)
        .Returns(documents.AsQueryable());
            _mockDocumentRepository.Setup(repo => repo.UpdateDocumentAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);

            _mockPropertyCategoryRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(category);
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);
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
            var category = new PropertyCategory { PropertyCategoryId = 1, CategoryId = 1, PropertyId = 1 };
            Document document = new Document { DocumentId = 1, PropertyCategory = category };

            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _documentController.DocumentRenameFormPartial(id) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_DocumentRenameForm", result.ViewName);
        }



        [Test]
        public async Task DocumentFileNameRename_DocumentNotFound_Fail()
        {
            var documentId = 1;
            var filename = "newname.txt";
            var viewModel = new RenameDocumentViewModel { FileName = filename };

            // Mock an empty collection
            _mockDocumentRepository.Setup(d => d.AllDocuments)
                .Returns(new List<Document>().AsQueryable());

            var result = await _documentController.DocumentFileNameRename(documentId, viewModel);

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
            PropertyCategory propertyCategory = new PropertyCategory { PropertyId = 1 };
            var document = new Document { DocumentId = documentId, FileName = oldFilename, PropertyCategory = propertyCategory };
            var viewModel = new RenameDocumentViewModel { FileName = newFilename };

            _mockDocumentRepository.Setup(d => d.AllDocuments)
                .Returns(new List<Document> { document }.AsQueryable());
            _mockAuthorizationService.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success);

            var result = await _documentController.DocumentFileNameRename(documentId, viewModel) as JsonResult;

            Assert.IsNotNull(result); 
            Assert.AreEqual(true, result.Value.GetType().GetProperty("success").GetValue(result.Value, null));  
            Assert.AreEqual("Document renamed successfully!", result.Value.GetType().GetProperty("message").GetValue(result.Value, null));  

          
            Assert.AreEqual(newFilename, document.FileName);
        }


        [Test]
        public async Task GetDocumentShareUrlPartial_DocumentNotFound_Fail()
        {
            var documentId = 1;
      

            _mockDocumentRepository.Setup(d => d.AllDocuments)
                .Returns(new List<Document>().AsQueryable());

            var result = await _documentController.GetDocumentShareUrlPartial(documentId);

            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Test]
        public async Task GetDocumentShareUrlPartial_DocumentExists_Success()
        {
            int id = 1;
            Document document = new Document { DocumentId = 1 };
            var viewModel = new ShareDocumentFormViewModel { Document = document };

            _mockDocumentRepository.Setup(d => d.AllDocuments).Returns(new List<Document> { document }.AsQueryable());

            var result = await _documentController.GetDocumentShareUrlPartial(id) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_GetDocumentShareUrl", result.ViewName);
        }

        [Test]
        public async Task ShareDocumentUrl_DocumentNotFound_Fail()
        {
            var documentId = 1;
            var email = "test@example.com";
            var url = "https://devstoreaccount1/test/category/test.pdf";
            var viewModel = new ShareDocumentFormViewModel { Email = email, Url = url };

            _mockDocumentRepository.Setup(d => d.AllDocuments)
                .Returns(new List<Document>().AsQueryable());

            var result = await _documentController.ShareDocumentUrl(documentId, viewModel);

            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Test]
        public async Task ShareDocumentUrl_EmailSendingFails_Fail()
        {
            var documentId = 1;
            var email = "test@example.com";
            var url = "https://devstoreaccount1/test/category/test.pdf";

            var document = new Document { DocumentId = documentId, FileName = "test.pdf" };
            var viewModel = new ShareDocumentFormViewModel { Email = email, Url = url, Document = document };

            _mockDocumentRepository.Setup(d => d.GetById(documentId))
                .ReturnsAsync(document);

            _renderer.Setup(v => v.RenderViewToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<HttpContext>()))
                .ReturnsAsync("Email Content");

            _mockEmailSender.Setup(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP error"));

            var result = await _documentController.ShareDocumentUrl(documentId, viewModel);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = False, message = Error sending email: SMTP error }"));
        }

        [Test]
        public async Task ShareDocumentUrl_EmailSentSuccessfully_Success()
        {
            var documentId = 1;
            var email = "test@example.com";
            var url = "https://devstoreaccount1/test/category/test.pdf";

            var document = new Document { DocumentId = documentId, FileName = "TestDocument.pdf" };
            var viewModel = new ShareDocumentFormViewModel { Email = email, Url = url, Document = document };

            _mockDocumentRepository.Setup(d => d.GetById(documentId)).ReturnsAsync(document);

            _renderer.Setup(v => v.RenderViewToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<HttpContext>()))
                .ReturnsAsync("Email Content");

            _mockEmailSender.Setup(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _documentController.ShareDocumentUrl(documentId, viewModel);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            dynamic value = jsonResult.Value.ToString();
            Assert.That(value.Equals("{ success = True, message = Document link shared successfully! }"));
        }

        [TearDown]
        public void Teardown()
        {
            _documentController.Dispose();
        }

    }


}

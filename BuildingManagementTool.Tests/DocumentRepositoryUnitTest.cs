using BuildingManagementTool.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

namespace BuildingManagementTool.Tests
{
    public class DocumentRepositoryUnitTest
    {
        private DbContextOptions<BuildingManagementToolDbContext> _options;
        private BuildingManagementToolDbContext _dbContext;
        private DocumentRepository _documentRepository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<BuildingManagementToolDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            _dbContext = new BuildingManagementToolDbContext(_options);
            _documentRepository = new DocumentRepository(_dbContext);
        }

        [Test]
        public async Task AddDocumentData_AddSuccess()
        {
            var document = new Document
            {
                DocumentId = 1,
                FileName = "text.txt",
                BlobName = "category/text.txt",
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow,
                FileImageUrl = "/imgs/text.svg"
            };

            await _documentRepository.AddDocumentData(document);
            var savedDocument = await _dbContext.Documents.FindAsync(1);
            Assert.That(savedDocument != null);
            Assert.That(savedDocument.FileName.Equals(document.FileName));
        }

        [Test]
        public async Task AddDocumentData_MissingProperties_ThrowArgument()
        {
            var document = new Document
            {
                DocumentId = 1,
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow
            };

            Assert.ThrowsAsync <DbUpdateException> (async () =>
            await _documentRepository.AddDocumentData(document));
        }

        [Test]
        public async Task AddDocumentData_NullObject_ThrowArgument()
        {
            var document = new Document();

            Assert.ThrowsAsync<DbUpdateException>(async () =>
            await _documentRepository.AddDocumentData(document));
        }

        [Test]
        public async Task DeleteDocumentData_Success()
        {
            var document = new Document
            {
                DocumentId = 1,
                FileName = "text.txt",
                BlobName = "category/text.txt",
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow,
                FileImageUrl = "/imgs/text.svg"
            };

            await _documentRepository.AddDocumentData(document);
            var result = await _documentRepository.DeleteDocumentData(document);
            var savedDocument = await _dbContext.Documents.FindAsync(1);
            Assert.That(savedDocument == null);
            Assert.True(result);
        }

        [Test]
        public async Task DeleteDocumentData_FileNotExist_Fail()
        {
            var document = new Document();

            var result = await _documentRepository.DeleteDocumentData(document);
            Assert.False(result);
        }

        [Test]
        public async Task GetById_ValidId_ReturnDocument()
        {
            var document = new Document
            {
                DocumentId = 1,
                FileName = "text.txt",
                BlobName = "category/text.txt",
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow,
                FileImageUrl = "/imgs/text.svg"
            };

            var document2 = new Document
            {
                DocumentId = 2,
                FileName = "text2.txt",
                BlobName = "category/text2.txt",
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow,
                FileImageUrl = "/imgs/text.svg"
            };

            await _documentRepository.AddDocumentData(document);
            await _documentRepository.AddDocumentData(document2);

            var savedDocument = await _documentRepository.GetById(1);
            Assert.That(savedDocument != null);
            Assert.That(savedDocument.DocumentId.Equals(1), "DocumentId should be 1");
            Assert.That(savedDocument.FileName.Equals("text.txt"), "Filename should be text.txt");
        }

        [Test]
        public async Task GetByPropertyCategoryId_ValidId_ReturnDocuments()
        {
            var document = new Document
            {
                DocumentId = 1,
                FileName = "text.txt",
                BlobName = "category/text.txt",
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow,
                FileImageUrl = "/imgs/text.svg",
                PropertyCategoryId = 1
            };

            var document2 = new Document
            {
                DocumentId = 2,
                FileName = "text2.txt",
                BlobName = "category/text2.txt",
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow,
                FileImageUrl = "/imgs/text.svg",
                PropertyCategoryId = 1
            };

            var document3 = new Document
            {
                DocumentId = 3,
                FileName = "text3.txt",
                BlobName = "category/text2.txt",
                ContentType = "text/plain",
                FileSize = 1,
                UploadDate = DateTime.UtcNow,
                FileImageUrl = "/imgs/text.svg",
                PropertyCategoryId = 2
            };

            await _documentRepository.AddDocumentData(document);
            await _documentRepository.AddDocumentData(document2);
            await _documentRepository.AddDocumentData(document3);

            var savedDocument = await _documentRepository.GetByPropertyCategoryId(1);
            Assert.That(savedDocument != null);
            Assert.That(savedDocument.Count().Equals(2), "Document count should be 2");
            Assert.That(savedDocument[0].FileName.Equals("text.txt"), "Filename should be text.txt");
            Assert.That(savedDocument[1].FileName.Equals("text2.txt"), "Filename should be text2.txt");
        }

        [Test]
        public async Task DeleteByPropertyId_ValidId_Success()
        {
            int id = 1;
            var propertyCategoriesList = new List<PropertyCategory>
            {
                new PropertyCategory { PropertyId = 1, CategoryId = 1, PropertyCategoryId = 1 },
                new PropertyCategory { PropertyId = 2, CategoryId = 2, PropertyCategoryId = 2 }
            };

            var documentList = new List<Document>
            {
                new Document { PropertyCategory = propertyCategoriesList[0], PropertyCategoryId = 1, DocumentId = 1, FileName = "Test Document 1", ContentType = "text/plain", BlobName = "property/category/Test-Document-1", FileSize = 2, FileImageUrl = "/imgs/text.svg" },
                new Document { PropertyCategory = propertyCategoriesList[1] ,PropertyCategoryId = 2, DocumentId = 2, FileName = "Test Document 2", ContentType = "text/plain", BlobName = "property/category/Test-Document-2", FileSize = 2, FileImageUrl = "/imgs/text.svg" }
            };

            await _dbContext.Documents.AddRangeAsync(documentList);
            await _dbContext.SaveChangesAsync();

            var initialList = await _dbContext.Documents.ToListAsync();

            await _documentRepository.DeleteByPropertyId(id);

            var newList = await _dbContext.Documents.ToListAsync();
            Assert.That(newList.Count, Is.EqualTo(1));
            Assert.That(newList[0].PropertyCategoryId, Is.EqualTo(documentList[1].PropertyCategoryId));
            Assert.That(newList[0].PropertyCategoryId, Is.EqualTo(documentList[1].PropertyCategoryId));
        }

        [Test]
        public async Task DeleteByPropertyId_NullId_ThrowEx()
        {
            int id = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _documentRepository.DeleteByPropertyId(id));
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }


    }
}
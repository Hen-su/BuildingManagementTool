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

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }


    }
}
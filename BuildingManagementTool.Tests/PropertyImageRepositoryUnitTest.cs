using BuildingManagementTool.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class PropertyImageRepositoryUnitTest
    {
        private DbContextOptions<BuildingManagementToolDbContext> _options;
        private BuildingManagementToolDbContext _dbContext;
        private PropertyImageRepository _propertyImageRepository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<BuildingManagementToolDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            _dbContext = new BuildingManagementToolDbContext(_options);
            _propertyImageRepository = new PropertyImageRepository(_dbContext);
        }

        [Test]
        public async Task AddPropertyImage_Valid_Success()
        {
            var image = new PropertyImage
            {
                Id = 1,
                FileName = "image1.jpg",
                BlobName = "user/property/images/image1.jpg",
                PropertyId = 1
            };

            await _propertyImageRepository.AddPropertyImage(image);
            var savedImage = await _dbContext.PropertyImages.FindAsync(1);
            Assert.That(savedImage != null);
            Assert.That(savedImage.FileName.Equals(image.FileName));
        }

        [Test]
        public async Task AddPropertyImage_Null_ThrowEx()
        {
            PropertyImage image = null;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.AddPropertyImage(image));
        }

        [Test]
        public async Task DeletePropertyImage_Valid_Success()
        {
            int id = 1;

            var imageList = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", PropertyId = 2}
            };

            _dbContext.PropertyImages.AddRange(imageList);
            await _dbContext.SaveChangesAsync();

            var initialList = await _dbContext.PropertyImages.ToListAsync();

            await _propertyImageRepository.DeletePropertyImage(imageList[0]);

            var newList = await _dbContext.PropertyImages.ToListAsync();
            Assert.That(newList.Count, Is.EqualTo(1));
            Assert.That(newList[0].PropertyId, Is.EqualTo(imageList[1].PropertyId));
            Assert.That(newList[0].FileName, Is.EqualTo(imageList[1].FileName));
        }

        [Test]
        public async Task DeletePropertyImage_Null_ThrowEx()
        {
            PropertyImage image = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.DeletePropertyImage(image));
        }

        [Test]
        public async Task DeleteByPropertyId_ValidId_Success()
        {
            int id = 1;
            var imageList = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 2}
            };

            await _dbContext.PropertyImages.AddRangeAsync(imageList);
            await _dbContext.SaveChangesAsync();

            var initialList = await _dbContext.PropertyImages.ToListAsync();

            await _propertyImageRepository.DeleteByPropertyId(id);

            var newList = await _dbContext.PropertyImages.ToListAsync();
            Assert.That(newList.Count, Is.EqualTo(1));
            Assert.That(newList[0].Id, Is.EqualTo(imageList[2].Id));
            Assert.That(newList[0].FileName, Is.EqualTo(imageList[2].FileName));
        }

        [Test]
        public async Task DeleteByPropertyId_NullId_ThrowEx()
        {
            int id = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.DeleteByPropertyId(id));
        }

        [Test]
        public async Task GetById_ValidId_ReturnPropertyImage()
        {
            var id = 1;
            var imageList = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 2}
            };
            await _dbContext.PropertyImages.AddRangeAsync(imageList);
            await _dbContext.SaveChangesAsync();

            var returned = await _propertyImageRepository.GetById(id);
            Assert.That(returned.Id, Is.EqualTo(1));
            Assert.That(returned.FileName, Is.EqualTo(imageList[0].FileName));
        }

        [Test]
        public async Task GetById_InvalidId_ThrowEx()
        {
            int id = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.GetById(id));
        }

        [Test]
        public async Task GetByPropertyId_ValidId_ReturnList()
        {
            var id = 1;
            var imageList = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 2}
            };
            await _dbContext.PropertyImages.AddRangeAsync(imageList);
            await _dbContext.SaveChangesAsync();

            var returnedList = await _propertyImageRepository.GetByPropertyId(id);
            Assert.That(returnedList.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetByPropertyId_InvalidId_ThrowEx()
        {
            int id = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.GetByPropertyId(id));
        }

        [Test]
        public async Task GetByFilename_ValidName_ReturnPropertyImage()
        {
            var id = 1;
            var name = "image1.jpg";
            var imageList = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", PropertyId = 2}
            };
            await _dbContext.PropertyImages.AddRangeAsync(imageList);
            await _dbContext.SaveChangesAsync();

            var returned = await _propertyImageRepository.GetByFileName(id, name);
            Assert.That(returned.Id, Is.EqualTo(1));
            Assert.That(returned.FileName, Is.EqualTo(imageList[0].FileName));
        }

        [Test]
        public async Task GetByFilename_InvalidName_ThrowEx()
        {
            int id = 1;
            string name = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.GetByFileName(id, name));
        }

        [Test]
        public async Task SetDisplayImage_ValidPropertyImage_SetDisplay()
        {
            var imageList = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", IsDisplay = false, PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", IsDisplay = false, PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", IsDisplay = false, PropertyId = 2}
            };
            await _dbContext.PropertyImages.AddRangeAsync(imageList);
            await _dbContext.SaveChangesAsync();

            var image = await _dbContext.PropertyImages.FindAsync(1);
            await _propertyImageRepository.SetDisplayImage(image);
            Assert.That(image.Id, Is.EqualTo(1));
            Assert.That(image.FileName, Is.EqualTo(imageList[0].FileName));
            Assert.That(image.IsDisplay, Is.True);
        }

        [Test]
        public async Task SetDisplayImage_InvalidPropertyImage_ThrowEx()
        {
            PropertyImage image = new PropertyImage();
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.SetDisplayImage(image));
        }

        [Test]
        public async Task RemoveDisplayImage_ValidPropertyImage_SetDisplay()
        {
            int id = 1;
            var imageList = new List<PropertyImage>
            {
                new PropertyImage {Id = 1, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", IsDisplay = true, PropertyId = 1},
                new PropertyImage {Id = 2, FileName = "image2.jpg", BlobName = "user/property/images/image2.jpg", IsDisplay = false, PropertyId = 1 },
                new PropertyImage {Id = 3, FileName = "image1.jpg", BlobName = "user/property/images/image1.jpg", IsDisplay = false, PropertyId = 2}
            };
            await _dbContext.PropertyImages.AddRangeAsync(imageList);
            await _dbContext.SaveChangesAsync();

            await _propertyImageRepository.RemoveDisplayImage(id);
            var returnedList = _dbContext.PropertyImages.Where(x => x.Id == id && x.IsDisplay == true).ToList();
            Assert.IsEmpty(returnedList);
        }

        [Test]
        public async Task RemoveDisplayImage_InvalidPropertyImage_ThrowEx()
        {
            int id = 0;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _propertyImageRepository.RemoveDisplayImage(id));
        }

        [TearDown]
        public async Task TearDown()
        {
            _dbContext.Dispose();
        }
    }
}

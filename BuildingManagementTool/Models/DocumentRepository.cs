﻿
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace BuildingManagementTool.Models
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly BuildingManagementToolDbContext _buildingManagementToolDbContext;
        public DocumentRepository(BuildingManagementToolDbContext buildingManagementToolDbContext)
        {
            _buildingManagementToolDbContext = buildingManagementToolDbContext;
        }

        public IEnumerable<Document> AllDocuments
        {
            get
            {
                return _buildingManagementToolDbContext.Documents.Include(d => d.PropertyCategory);
            }
        }

        public async Task AddDocumentData(Document file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file), "Document cannot be null.");
            }
            await _buildingManagementToolDbContext.AddAsync(file);
            await _buildingManagementToolDbContext.SaveChangesAsync();
        }

        public async Task<Document> GetById(int id)
        {
            return await _buildingManagementToolDbContext.Documents.Include(d => d.PropertyCategory).FirstOrDefaultAsync(d => d.DocumentId == id);
        }

        public async Task<List<Document>> GetByPropertyCategoryId(int id)
        {
            return await _buildingManagementToolDbContext.Documents.Where(d => d.PropertyCategoryId == id).ToListAsync();
        }

        public async Task UpdateDocumentAsync(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Document cannot be null.");
            }
            _buildingManagementToolDbContext.Documents.Update(document);
            await _buildingManagementToolDbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteDocumentData(Document document)
        {
            if (document != null) 
            {
                try
                {
                    _buildingManagementToolDbContext.Documents.Remove(document);
                    await _buildingManagementToolDbContext.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public async Task DeleteByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property Id cannot be null.");
            }
            var documentList = _buildingManagementToolDbContext.Documents.Include(d => d.PropertyCategory).Where(d => d.PropertyCategory.PropertyId == id);
            _buildingManagementToolDbContext.Documents.RemoveRange(documentList);
            await _buildingManagementToolDbContext.SaveChangesAsync();
        }

        public async Task<List<Document>> GetByPropertyId(int id)
        {
            if (id == null || id == 0)
            {
                throw new ArgumentNullException("Property Id cannot be null.");
            }
            var propertyDocuments = await _buildingManagementToolDbContext.Documents.Include(d => d.PropertyCategory).Where(d => d.PropertyCategory.PropertyId == id).ToListAsync();
            return propertyDocuments;
        }

        public async Task UpdateByList(List<Document> documents)
        {
            if (documents.Any())
            {
                _buildingManagementToolDbContext.Documents.UpdateRange(documents);
                await _buildingManagementToolDbContext.SaveChangesAsync();
            }
        }
    }
}

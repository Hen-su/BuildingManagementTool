﻿using BuildingManagementTool.Models;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagementTool.Controllers
{
    [Authorize]
    public class PropertyCategoryController : Controller
    {
        private readonly IPropertyCategoryRepository _propertyCategoryRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDocumentRepository _documentRepository;
        public PropertyCategoryController(IPropertyCategoryRepository propertyCategoryRepository, IPropertyRepository propertyRepository, ICategoryRepository categoryRepository, IDocumentRepository documentRepository)
        {
            _propertyCategoryRepository = propertyCategoryRepository;
            _propertyRepository = propertyRepository;
            _categoryRepository = categoryRepository;
            _documentRepository = documentRepository;
        }
        public async Task<IActionResult> Index(int id)
        {
            //test id
            id = 1;
            var property = await _propertyRepository.GetById(id);
            if (property == null) 
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }
            var categories = await _propertyCategoryRepository.GetByPropertyId(id);
            var img = "/imgs/sample-house.jpeg";
            var viewModel = new CategoryViewModel(categories, img, property);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategoryCanvas(int id)
        {
            //test id
            id = 1;
            var property = await _propertyRepository.GetById(id);
            if (property == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }
            var categories = await _propertyCategoryRepository.GetByPropertyId(id);
            var img = "/imgs/sample-house.jpeg";
            var viewModel = new CategoryViewModel(categories, img, property);
            return PartialView("_CategoryCanvas", viewModel);
        }

        //test id being used in call function
        [HttpGet]
        public async Task<IActionResult> CategoryFormPartial(int id)
        {
            var currentProperty = await _propertyRepository.GetById(id);
            if (currentProperty == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                }); 
            }
            var categories = await _categoryRepository.Categories();
            var categoryFormViewModel = new CategoryFormViewModel(categories, id, null);
            return PartialView("_AddCategoryForm", categoryFormViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int id, string name)
        {
            var currentProperty = await _propertyRepository.GetById(id);
            if (currentProperty == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "Could not find matching property"
                });
            }
            if (name != null) 
            { 
                var categories = await _categoryRepository.Categories();
                var existing = categories.FirstOrDefault(c => c.CategoryName == name);
                if (existing != null)
                {
                    var newCategory = new PropertyCategory
                    {
                        PropertyId = id,
                        CategoryId = existing.CategoryId
                    };
                    await _propertyCategoryRepository.AddPropertyCategory(newCategory);
                    return Json(new { success = true });
                }
                else
                {
                    var newCategory = new PropertyCategory
                    {
                        PropertyId = id,
                        CustomCategory = name
                    };
                    await _propertyCategoryRepository.AddPropertyCategory(newCategory);
                    return Json(new { success = true });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred when adding new category. Please try again"
            });
        }

        public async Task<IActionResult> DeleteConfirmationPartial(int id)
        {
            var category = await _propertyCategoryRepository.GetById(id);
            if (category == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "Could not find matching category"
                });
            }
            return PartialView("_CategoryDeleteConfirmation", category);
        }

        public async Task<IActionResult> EditCategoryFormPartial(int id, int currentCategoryId)
        {
            
            var currentProperty = await _propertyRepository.GetById(id);
            if (currentProperty == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "The selected property could not be found"
                });
            }

            var currentCategory = await _propertyCategoryRepository.GetById(currentCategoryId);
            if (currentCategory == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    success = false,
                    message = "Could not find matching category"
                });
            }
            var categories = await _categoryRepository.Categories();
            var categoryFormViewModel = new CategoryFormViewModel(categories, id, currentCategory);
            return PartialView("_EditCategoryForm", categoryFormViewModel);
        }
    }
}

using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.CategoryDTOs;
using Core.Interfaces;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CatStoreAPI.Controllers
{
    [Route("api/Category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ReorderCategories reorderCategoriesHelper;

        public CategoryController(IUnitOfWork _unitOfWork, IMapper _mapper, ReorderCategories _reorderCategoriesHelper)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
            reorderCategoriesHelper = _reorderCategoriesHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await unitOfWork.Categories.GetAllAsync();
            categories.OrderBy(x => x.DisplayOrder).ToList();

            return Ok(categories);
        }

        [HttpGet("{Id}", Name = "GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            try
            {
                var category = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryCreatDTO categoryDTO)
        {
            // Check if the Name or the display order already exists 
            var existsName = await unitOfWork.Categories.GetSingleAsync(x => x.Name.ToLower() == categoryDTO.Name.ToLower());
            var existsDisplayOrder = await unitOfWork.Categories.GetSingleAsync(x => x.DisplayOrder == categoryDTO.DisplayOrder);

            if (existsName is not null)
                ModelState.AddModelError("", "Category already exists!");
            
            if (existsDisplayOrder is not null)
                ModelState.AddModelError("", "Display order already exists!");

            if (ModelState.IsValid)
            {
                var createdCategory = mapper.Map<Category>(categoryDTO);

                // Get the current maximum display order and assign the following order to the new category
                var categories = await unitOfWork.Categories.GetAllAsync();
                createdCategory.DisplayOrder = categories.Max(x => x.DisplayOrder) + 1;

                createdCategory = await unitOfWork.Categories.AddAsync(createdCategory);

                await unitOfWork.SaveChangesAsync();
                return Ok(createdCategory);
            }
            else
                return BadRequest(ModelState);

        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> EditCategory(int Id, CategoryUpdateDTO categoryUpdateDTO)
        {
            // Check if the new Name or the new display order already exists 
            var existsName = await unitOfWork.Categories.GetSingleAsync(x => x.Name.ToLower() == categoryUpdateDTO.Name.ToLower());
            var existsDisplayOrder = await unitOfWork.Categories.GetSingleAsync(x => x.DisplayOrder == categoryUpdateDTO.DisplayOrder);

            if (existsDisplayOrder is not null)
                ModelState.AddModelError("", "Display Order already exists!");

            if (existsName is not null)
                ModelState.AddModelError("", "Category already exists!");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentCategory = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                    var editedCategory = mapper.Map<Category>(categoryUpdateDTO);
                    editedCategory = await unitOfWork.Categories.UpdateAsync(Id, editedCategory);

                    await unitOfWork.SaveChangesAsync();
                    return Ok(editedCategory);
                }
                catch (Exception ex)
                {
                    return NotFound(ex.Message);
                }
            }
            else
                return BadRequest(ModelState);
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> RemoveCategory(int Id)
        {
            try
            {
                var removedCategory = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                await unitOfWork.Categories.RemoveAsync(x => x.Id == Id);

                await unitOfWork.SaveChangesAsync();
                return Ok(removedCategory);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

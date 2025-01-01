using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.CategoryDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Net;

namespace CatStoreAPI.Controllers
{
    [Route("api/Category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IReorderCategoriesService reorderCategoriesService;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly APIResponse response;

        public CategoryController(IUnitOfWork _unitOfWork, IMapper _mapper, IReorderCategoriesService _reorderCategoriesService, IOutputCacheStore outputCacheStore)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
            reorderCategoriesService = _reorderCategoriesService;
            this.outputCacheStore = outputCacheStore;
            this.response = new APIResponse();
        }

        /// <summary>
        /// Retrieves a list of all categories.
        /// </summary>
        /// <returns>An ActionResult containing an APIResponse with the list of categories.</returns>
        /// <response code="200">Categories retrieved successfully.</response>
        [HttpGet]
        [OutputCache(Duration = 60, Tags = ["Categories"])]
        public async Task<ActionResult<APIResponse>> GetAllCategories()
        {
            var categories = await unitOfWork.Categories.GetAllAsync();
            categories = categories.OrderBy(x => x.DisplayOrder).ToList();

            response.Result = categories;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="Id">The unique identifier of the category.</param>
        /// <returns>An ActionResult containing an APIResponse with the requested category.</returns>
        /// <response code="200">Category retrieved successfully.</response>
        /// <response code="400">Bad request due to invalid data.</response>
        /// <response code="404">Category does not exist.</response>
        [HttpGet("{Id}", Name = "GetCategoryById")]
        [OutputCache(Duration = 120, VaryByRouteValueNames = ["Id"], Tags = ["Category"])]
        public async Task<ActionResult<APIResponse>> GetCategoryById(int Id)
        {
            try
            {
                var category = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);
                if(category is null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Category does not exist.");
                    return NotFound(response);
                }
                response.Result = category;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDTO">An object containing the details of the category to create.</param>
        /// <returns>An ActionResult containing an APIResponse with the created category.</returns>
        /// <response code="200">Category created successfully.</response>
        /// <response code="400">Bad request due to validation errors.</response>
        /// <remarks>Requires administrator privileges.</remarks>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> CreateCategory(CategoryCreatDTO categoryDTO)
        {
            // Check if the Name already exists 
            var existsName = await unitOfWork.Categories.GetSingleAsync(x => x.Name.ToLower() == categoryDTO.Name.ToLower());

            if (existsName is not null)
                ModelState.AddModelError("", "Category already exists!");

            if (ModelState.IsValid)
            {
                var createdCategory = mapper.Map<Category>(categoryDTO);

                // Get the current maximum display order and assign the next order to the new category
                var categories = await unitOfWork.Categories.GetAllAsync();
                createdCategory.DisplayOrder = categories.DefaultIfEmpty().Max(x => x?.DisplayOrder ?? 0) + 1;

                createdCategory = await unitOfWork.Categories.AddAsync(createdCategory);

                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync("Categories", HttpContext.RequestAborted);

                response.Result = createdCategory;
                response.StatusCode = HttpStatusCode.Created;
                response.IsSuccess = true;
                return Ok(response);
            }

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="Id">The unique identifier of the category to update.</param>
        /// <param name="categoryUpdateDTO">An object containing the updated category details.</param>
        /// <returns>An ActionResult containing an APIResponse with the updated category.</returns>
        /// <response code="200">Category updated successfully.</response>
        /// <response code="400">Bad request due to validation errors.</response>
        /// <response code="404">Category not found.</response>
        /// <remarks>Requires administrator privileges.</remarks>
        [HttpPut("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> EditCategory(int Id, CategoryUpdateDTO categoryUpdateDTO)
        {
            // Check if the new Name already exists 
            if (categoryUpdateDTO.Name is not null)
            {
                var existsName = await unitOfWork.Categories
                    .GetSingleAsync(x => x.Name.ToLower() == categoryUpdateDTO.Name.ToLower());

                if (existsName is not null && existsName.Id != Id)
                    ModelState.AddModelError("", "Category already exists!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentCategory = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                    var editedCategory = mapper.Map<Category>(categoryUpdateDTO);
                    editedCategory = await unitOfWork.Categories.UpdateAsync(Id, editedCategory);

                    await unitOfWork.SaveChangesAsync();

                    await outputCacheStore.EvictByTagAsync("Categories", HttpContext.RequestAborted);
                    await outputCacheStore.EvictByTagAsync($"Category-{Id}", HttpContext.RequestAborted);

                    response.Result = editedCategory;
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.Errors.Add(ex.Message);
                    return NotFound(response);
                }
            }

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }

        /// <summary>
        /// Deletes an existing category.
        /// </summary>
        /// <param name="Id">The unique identifier of the category to delete.</param>
        /// <returns>An ActionResult containing an APIResponse with details of the deleted category.</returns>
        /// <response code="200">Category deleted successfully.</response>
        /// <response code="404">Category not found.</response>
        /// <remarks>Requires administrator privileges.</remarks>
        [HttpDelete("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> RemoveCategory(int Id)
        {
            try
            {
                var removedCategory = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                // Removing a category leads to reordering the display order of the others.
                await reorderCategoriesService.ReorderOnRemoveAsync(removedCategory.DisplayOrder);

                await unitOfWork.Categories.RemoveAsync(x => x.Id == Id);

                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync("Categories", HttpContext.RequestAborted);
                await outputCacheStore.EvictByTagAsync($"Category-{Id}", HttpContext.RequestAborted);

                response.Result = removedCategory;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return NotFound(response);
            }
        }
    }
}
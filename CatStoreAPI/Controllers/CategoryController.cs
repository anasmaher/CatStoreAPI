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

        [HttpGet("{Id}", Name = "GetCategoryById")]
        [OutputCache(Duration = 120, VaryByRouteValueNames = ["Id"], Tags = ["Category"])]
        public async Task<ActionResult<APIResponse>> GetCategoryById(int Id)
        {
            try
            {
                var category = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> CreateCategory(CategoryCreatDTO categoryDTO)
        {
            // Check if the Name or the display order already exists 
            var existsName = await unitOfWork.Categories.GetSingleAsync(x => x.Name.ToLower() == categoryDTO.Name.ToLower());

            if (existsName is not null)
                ModelState.AddModelError("", "Category already exists!");

            if (ModelState.IsValid)
            {
                var createdCategory = mapper.Map<Category>(categoryDTO);

                // Get the current maximum display order and assign the following order to the new category
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

        [HttpPut("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> EditCategory(int Id, CategoryUpdateDTO categoryUpdateDTO)
        {
            // Check if the new Name or the new display order already exists 
            if(categoryUpdateDTO.Name is not null)
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

        [HttpDelete("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> RemoveCategory(int Id)
        {
            try
            {
                var removedCategory = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                // removing a category leads to reordering the display order of the others.
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

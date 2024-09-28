using AutoMapper;
using Azure;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.CategoryDTOs;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly APIResponse response;

        public CategoryController(IUnitOfWork _unitOfWork, IMapper _mapper, IReorderCategoriesService _reorderCategoriesService)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
            reorderCategoriesService = _reorderCategoriesService;
            this.response = new APIResponse();
        }

        [HttpGet]
        [Authorize]
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

                response.Result = createdCategory;
                response.StatusCode = HttpStatusCode.Created;
                response.IsSuccess = true;
                return Ok(response);
            }
            return BadRequest(ModelState);

        }

        [HttpPut("{Id}")]
        public async Task<ActionResult<APIResponse>> EditCategory(int Id, CategoryUpdateDTO categoryUpdateDTO)
        {
            // Check if the new Name or the new display order already exists 
            var existsName = await unitOfWork.Categories
                .GetSingleAsync(x => x.Name.ToLower() == categoryUpdateDTO.Name.ToLower());

            if (existsName is not null && existsName.Id != Id)
                ModelState.AddModelError("", "Category already exists!");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentCategory = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                    var editedCategory = mapper.Map<Category>(categoryUpdateDTO);
                    editedCategory = await unitOfWork.Categories.UpdateAsync(Id, editedCategory);

                    await unitOfWork.SaveChangesAsync();

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
            return BadRequest(ModelState);
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult<APIResponse>> RemoveCategory(int Id)
        {
            try
            {
                var removedCategory = await unitOfWork.Categories.GetSingleAsync(x => x.Id == Id);

                // removing a category leads to reordering the display order of the others.
                await reorderCategoriesService.ReorderOnRemoveAsync(removedCategory.DisplayOrder);

                await unitOfWork.Categories.RemoveAsync(x => x.Id == Id);

                await unitOfWork.SaveChangesAsync();

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

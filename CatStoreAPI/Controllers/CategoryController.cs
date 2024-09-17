﻿using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.CategoryDTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CatStoreAPI.Controllers
{
    [Route("api/Category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepo;
        private readonly IMapper mapper;

        public CategoryController(ICategoryRepository _categoryRepo, IMapper _mapper)
        {
            categoryRepo = _categoryRepo;
            mapper = _mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryRepo.GetAllAsync();

            return Ok(categories);
        }

        [HttpGet("{Id}", Name = "GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            try
            {
                var category = await categoryRepo.GetSingleAsync(x => x.Id == Id);

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
            var existsName = await categoryRepo.GetSingleAsync(x => x.Name.ToLower() == categoryDTO.Name.ToLower());
            var existsDisplayOrder = await categoryRepo.GetSingleAsync(x => x.DisplayOrder == categoryDTO.DisplayOrder);

            if (existsName is not null)
                ModelState.AddModelError("", "Category already exists!");
            
            if (existsDisplayOrder is not null)
                ModelState.AddModelError("", "Display order already exists!");

            if (ModelState.IsValid)
            {
                var createdCategory = mapper.Map<Category>(categoryDTO);
                createdCategory = await categoryRepo.AddAsync(createdCategory);

                return Ok(createdCategory);
            }
            else
                return BadRequest(ModelState);

        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> EditCategory(int Id, CategoryCreatDTO categoryUpdateDTO)
        {
            var existsName = await categoryRepo.GetSingleAsync(x => x.Name.ToLower() == categoryUpdateDTO.Name.ToLower());
            var existsDisplayOrder = await categoryRepo.GetSingleAsync(x => x.DisplayOrder == categoryUpdateDTO.DisplayOrder);

            if (existsDisplayOrder is not null)
                ModelState.AddModelError("", "Display Order already exists!");

            if (existsName is not null)
                ModelState.AddModelError("", "Category already exists!");

            if (ModelState.IsValid)
            {
                try
                {
                    var editedCategory = mapper.Map<Category>(categoryUpdateDTO);
                    editedCategory = await categoryRepo.UpdateAsync(Id, editedCategory);

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
                var removedCategory = await categoryRepo.GetSingleAsync(x => x.Id == Id);

                await categoryRepo.RemoveAsync(x => x.Id == Id);
                return Ok(removedCategory);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

using AutoMapper;
using CatStoreAPI.CatStore.Models;
using CatStoreAPI.DTO;
using Core.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

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
            IEnumerable<Category> categories = await categoryRepo.GetAllAsync();
            return Ok(mapper.Map<List<CategoryDTO>>(categories));
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            try
            {
                var category = await categoryRepo.GetSingleAsync(x => x.Id == Id);

                return Ok(mapper.Map<CategoryDTO>(category));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody]CategoryDTO categoryDTO)
        {
            return Ok(categoryDTO);
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> EditCategory(int Id, [FromForm]CategoryDTO categoryDTO)
        {
            return Ok();
        }

        [HttpPatch("{Id}")]
        public async Task<IActionResult> EditCategoryAttribute(int Id, JsonPatchDocument<CategoryDTO> categoryPatchDTO)
        {
            return Ok();
        }
    }
}

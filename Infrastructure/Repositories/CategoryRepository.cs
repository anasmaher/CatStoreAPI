using CatStoreAPI.Core.Models;
using Core.Interfaces;
using Infrastructure.DataBase;
using Infrastructure.Services;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly AppDbContext dbContext;
        private readonly IReorderCategoriesService reorderCategoriesService;

        public CategoryRepository(AppDbContext _dbContext, IReorderCategoriesService _reorderCategoriesService) : base(_dbContext)
        {
            dbContext = _dbContext;
            reorderCategoriesService = _reorderCategoriesService;
        }

        public async Task<Category> UpdateAsync(int Id, Category category)
        {
            var UpdatedCategory = await GetSingleAsync(x => x.Id == Id);

            if (UpdatedCategory is not null)
            {
                UpdatedCategory.Name = category.Name;
                UpdatedCategory.ImageUrl = category.ImageUrl;

                await reorderCategoriesService.ReorderOnUpdateAsync(UpdatedCategory, category.DisplayOrder, UpdatedCategory.DisplayOrder);

                return UpdatedCategory;
            }
            else
            {
                throw new Exception("Category not found!");
            }
        }
    }
}

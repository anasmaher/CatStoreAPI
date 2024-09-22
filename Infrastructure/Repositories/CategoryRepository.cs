﻿using CatStoreAPI.Core.Models;
using Core.Interfaces;
using Infrastructure.DataBase;
using Infrastructure.Helpers;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly AppDbContext dbContext;
        private readonly ReorderCategoriesHelper reorderCategoriesHelper;

        public CategoryRepository(AppDbContext _dbContext, ReorderCategoriesHelper _reorderCategoriesHelper) : base(_dbContext)
        {
            dbContext = _dbContext;
            reorderCategoriesHelper = _reorderCategoriesHelper;
        }

        public async Task<Category> UpdateAsync(int Id, Category category)
        {
            var UpdatedCategory = await GetSingleAsync(x => x.Id == Id);

            if (UpdatedCategory is not null)
            {
                UpdatedCategory.Name = category.Name;
                UpdatedCategory.ImageUrl = category.ImageUrl;

                await reorderCategoriesHelper.ReorderOnUpdateAsync(UpdatedCategory, category.DisplayOrder, UpdatedCategory.DisplayOrder);

                return UpdatedCategory;
            }
            else
            {
                throw new Exception("Category not found!");
            }
        }
    }
}

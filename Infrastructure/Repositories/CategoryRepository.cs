using CatStoreAPI.Core.Models;
using Core.Interfaces;
using Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly AppDbContext dbContext;

        public CategoryRepository(AppDbContext _dbContext) : base(_dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task<Category> UpdateAsync(int Id, Category category)
        {
            var UpdatedCategory = await GetSingleAsync(x => x.Id == Id);

            if (UpdatedCategory is not null)
            {
                UpdatedCategory.Name = category.Name;
                UpdatedCategory.ImageUrl = category.ImageUrl;

                dbContext.SaveChanges();

                return UpdatedCategory;
            }
            else
            {
                throw new Exception("Category not found!");
            }
        }
    }
}

using CatStoreAPI.Core.Models;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class ReorderCategories
    {
        private readonly AppDbContext dbContext;

        public ReorderCategories(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task ReorderAsync(Category category, int newOrder, int oldOrder)
        {
            if (oldOrder == newOrder) return;

            if (oldOrder > newOrder)
            {
                category.DisplayOrder = newOrder;

                var categories = await dbContext.Categories
                    .Where(x => x.DisplayOrder >= newOrder && x.DisplayOrder < oldOrder)
                    .Skip(1)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].DisplayOrder++;
                }
            }
            else
            {
                category.DisplayOrder = newOrder;

                var categories = await dbContext.Categories
                    .Where(x => x.DisplayOrder >= oldOrder && x.DisplayOrder <= newOrder)
                    .SkipLast(1)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

                for(int i = 0; i <categories.Count; i++)
                {
                    categories[i].DisplayOrder--;
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}

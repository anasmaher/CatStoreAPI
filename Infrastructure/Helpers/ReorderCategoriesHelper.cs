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
    public class ReorderCategoriesHelper
    {
        private readonly AppDbContext dbContext;

        public ReorderCategoriesHelper(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task ReorderOnRemoveAsync(int removedDisplayOrder)
        {
            var categories = await dbContext.Categories
                .Where(x => x.DisplayOrder > removedDisplayOrder)
                .ToListAsync();

            foreach (var category in categories)
            {
                category.DisplayOrder--;
            }
        }

        public async Task ReorderOnUpdateAsync(Category category, int newOrder, int oldOrder)
        {
            if (oldOrder == newOrder) return;

            if (oldOrder > newOrder)
            {
                var categories = await dbContext.Categories
                    .Where(x => x.DisplayOrder >= newOrder && x.DisplayOrder < oldOrder)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].DisplayOrder++;
                }

                category.DisplayOrder = newOrder;
            }
            else
            {
                var categories = await dbContext.Categories
                    .Where(x => x.DisplayOrder >= oldOrder && x.DisplayOrder <= newOrder)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].DisplayOrder--;
                }

                category.DisplayOrder = newOrder;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}

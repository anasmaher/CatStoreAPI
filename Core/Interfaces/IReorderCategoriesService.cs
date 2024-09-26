using CatStoreAPI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IReorderCategoriesService
    {
        public Task ReorderOnRemoveAsync(int removedDisplayOrder);

        public Task ReorderOnUpdateAsync(Category category, int newOrder, int oldOrder);
    }
}

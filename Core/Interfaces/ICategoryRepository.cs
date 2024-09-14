using CatStoreAPI.CatStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        public Task UpdateAsync(Category obj);
    }
}

using CatStoreAPI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        public Task<Category> UpdateAsync(int Id, Category obj);
    }
}

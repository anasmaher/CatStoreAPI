using CatStoreAPI.Core.Models;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IShoppingCartRepository ShoppingCarts { get; }

        Task<Product> AddProductWithNewCategoryAsync(Product product, string categoryName);

        Task SaveChangesAsync();
    }
}

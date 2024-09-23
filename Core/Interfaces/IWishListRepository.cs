using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IWishListRepository : IRepository<WishList>
    {
        public Task<WishList> GetWishListWithProductsAsync(int id);

        public Task<WishList> CreateWishListAsync();

        public Task<Product> AddWishlistProductAsync(int wishlistId, int productId);

        public Task<Product> RemoveWishListItemAsync(int wishlistId, int productId);

    }
}

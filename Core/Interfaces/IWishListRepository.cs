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
        public Task<WishList> GetWishListWithProductsAsync(string userId);

        public Task<WishList> CreateWishListAsync(string userId);

        public Task<Product> AddWishlistProductAsync(string UserId, int productId);

        public Task<bool> RemoveWishListProductAsync(string userId, int productId);
    }
}

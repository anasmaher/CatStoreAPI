using CatStoreAPI.Core.Models;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        public Task<ShoppingCart> GetCartWithItemsAsync(int id);

        public Task<ShoppingCartItem> GetCartItemByIdAsync(int id);

        public Task<ShoppingCartItem> AddItemAsync(int cartId, int ProductId);

        public Task<ShoppingCartItem> UpdateCartItemAsync(int itemId, int quantity);

        public Task<ShoppingCartItem> RemoveCartItemAsync(int itemId);

    }
}

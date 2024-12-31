using Core.Models;

namespace Core.Interfaces
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        public Task<ShoppingCart> GetCartWithItemsAsync(string id);

        public Task<ShoppingCartItem> GetCartItemByIdAsync(int id);

        public Task<ShoppingCartItem> AddItemAsync(string userId, int ProductId, int quantity);

        public Task<ShoppingCartItem> UpdateCartItemAsync(string userId, int itemId, int quantity);

        public Task<bool> RemoveCartItemAsync(string userId, int itemId);

    }
}

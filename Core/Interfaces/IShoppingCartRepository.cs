using Core.Models;

namespace Core.Interfaces
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        public Task<ShoppingCart> GetCartWithItemsAsync(int id);

        public Task<ShoppingCartItem> GetCartItemByIdAsync(int id);

        public Task<ShoppingCartItem> AddItemAsync(int cartId, int ProductId, int quantity);

        public Task<ShoppingCartItem> UpdateCartItemAsync(int itemId, int quantity);

        public Task<ShoppingCartItem> RemoveCartItemAsync(int itemId);

    }
}

using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly AppDbContext dbContext;

        public ShoppingCartRepository(AppDbContext _dbContext) : base(_dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task<ShoppingCart> CreateCartAsync(string userId)
        {
            var cart = new ShoppingCart
            {
                userId = userId
            };

            await dbContext.ShoppingCarts.AddAsync(cart);
            await dbContext.SaveChangesAsync();

            return cart;
        }

        // Get the cart with items for a specific user
        public async Task<ShoppingCart> GetCartWithItemsAsync(string userId)
        {
            var cart = await dbContext.ShoppingCarts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.userId == userId);

            if (cart is null)
                cart = await CreateCartAsync(userId);

            return cart;
        }

        public async Task<ShoppingCartItem> GetCartItemByIdAsync(int itemId)
        {
            var item = await dbContext.Set<ShoppingCartItem>()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            return item;
        }

        // Add an item to the cart for a specific user
        public async Task<ShoppingCartItem> AddItemAsync(string userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var cart = await GetCartWithItemsAsync(userId);
            if (cart is null)
            {
                cart = await CreateCartAsync(userId);
            }

            var existingCartItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (existingCartItem is null)
            {
                var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
                if (product is null)
                    throw new ArgumentException("Product not found.");

                var cartItem = new ShoppingCartItem
                {
                    ShoppingCartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                };

                await dbContext.ShoppingCartItems.AddAsync(cartItem);
                cart.Items.Add(cartItem);
            }
            else
            {
                existingCartItem.Quantity += quantity;
                dbContext.ShoppingCartItems.Update(existingCartItem);
            }

            await dbContext.SaveChangesAsync();
            return existingCartItem ?? cart.Items.FirstOrDefault(x => x.ProductId == productId);
        }

        // Update the quantity of a cart item
        public async Task<ShoppingCartItem> UpdateCartItemAsync(string userId, int itemId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var cart = await GetCartWithItemsAsync(userId);
            var item = cart.Items.FirstOrDefault(x => x.Id == itemId);

            if (item is null)
                throw new ArgumentException("Cart item not found.");

            item.Quantity = quantity;
            dbContext.ShoppingCartItems.Update(item);

            await dbContext.SaveChangesAsync();
            return item;
        }

        // Remove a cart item
        public async Task<bool> RemoveCartItemAsync(string userId, int itemId)
        {
            var cart = await GetCartWithItemsAsync(userId);
            var item = cart.Items.FirstOrDefault(x => x.Id == itemId);

            if (item is null)
                return false;

            cart.Items.Remove(item);
            dbContext.ShoppingCartItems.Remove(item);

            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}
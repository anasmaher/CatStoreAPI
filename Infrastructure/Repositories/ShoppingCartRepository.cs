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

        public async Task<ShoppingCart> CreateCartAsync()
        {
            var cart = new ShoppingCart();
            await dbContext.ShoppingCarts.AddAsync(cart);

            return cart;
        }

        public async Task<ShoppingCart> GetCartWithItemsAsync(int id)
        {
            var cart = await dbContext.ShoppingCarts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (cart is null)
                cart = await CreateCartAsync();

            return cart;
        }

        public async Task<ShoppingCartItem> GetCartItemByIdAsync(int id)
        {
            var item = await dbContext.Items.FirstOrDefaultAsync(x => x.Id == id);

            return item;
        }

        public async Task<ShoppingCartItem> AddItemAsync(int cartId, int ProductId, int quantity)
        {
            var cart = await GetCartWithItemsAsync(cartId);

            if (cart is null)
                cart = await CreateCartAsync();
            

            var itemHasProduct = await dbContext.Items
                .FirstOrDefaultAsync(x => x.ShoppingCartId == cartId && x.ProductId == ProductId);

            var item = new ShoppingCartItem();

            if (itemHasProduct is null)
            {
                var currentProduct = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == ProductId);

                item = new ShoppingCartItem()
                {
                    ProductId = ProductId,
                    Product = currentProduct,
                    Quantity = quantity,
                    ShoppingCartId = cartId,
                    ShoppingCart = cart
                };

                cart.Items.Add(item);

                await dbContext.Items.AddAsync(item);
            }
            else
            {
                item = await dbContext.Items
                    .FirstOrDefaultAsync(x => x.ProductId == ProductId && x.ShoppingCartId == cartId);
                
                await UpdateCartItemAsync(item.Id, item.Quantity + quantity);
            }

            return item;
        }

        public async Task<ShoppingCartItem> UpdateCartItemAsync(int itemId, int quantity)
        {
            var item = await GetCartItemByIdAsync(itemId);

            var cart = await dbContext.ShoppingCarts.FirstOrDefaultAsync(x => x.Id == item.ShoppingCartId);
            
            item.Quantity = quantity; 

            dbContext.Items.Update(item);

            return item;
        }

        public async Task<ShoppingCartItem> RemoveCartItemAsync(int itemId)
        {
            var item = await dbContext.Items.FirstOrDefaultAsync(x => x.Id == itemId);
            var cart = item.ShoppingCart;

            dbContext.Items.Remove(item);

            return item;
        }
    }
}
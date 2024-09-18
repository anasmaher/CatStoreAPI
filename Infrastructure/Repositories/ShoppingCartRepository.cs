using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            await dbContext.SaveChangesAsync();
            return cart;
        }

        public async Task<ShoppingCart> GetCartWithItemsAsync(int id)
        {
            var cart = await dbContext.ShoppingCarts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            return cart;
        }

        public async Task<ShoppingCartItem> GetCartItemByIdAsync(int id)
        {
            var item = await dbContext.Items.FirstOrDefaultAsync(x => x.Id == id);

            return item;
        }

        public async Task<ShoppingCartItem> AddItemAsync(int cartId, int ProductId)
        {
            var cart = await GetCartWithItemsAsync(cartId);

            if (cart is null)
            {
                cart = await CreateCartAsync();
            }

            var product = await dbContext.Products
                .FirstOrDefaultAsync(x => x.ShoppingCartId == cartId && x.Id == ProductId);

            var item = new ShoppingCartItem();

            if (product is null)
            {
                item = new ShoppingCartItem()
                {
                    ProductId = ProductId,
                    Product = product,
                    Quantity = 1,
                    ShoppingCartId = cartId,
                    ShoppingCart = cart
                };

                cart.Items.Add(item);
                await dbContext.Items.AddAsync(item);

                await dbContext.SaveChangesAsync();
            }
            else
            {
                item = await dbContext.Items
                    .FirstOrDefaultAsync(x => x.ProductId == ProductId && x.ShoppingCartId == cartId);
                
                await UpdateCartItemAsync(item.Id, item.Quantity + 1);
            }

            return item;
        }

        public async Task<ShoppingCartItem> UpdateCartItemAsync(int itemId, int quantity)
        {
            var item = await GetCartItemByIdAsync(itemId);

            item.Quantity = quantity;

            dbContext.Items.Update(item);
            await dbContext.SaveChangesAsync();

            return item;
        }

        public async Task<ShoppingCartItem> RemoveCartItemAsync(int itemId)
        {
            var item = dbContext.Items.FirstOrDefault(x => x.Id == itemId);

            dbContext.Items.Remove(item);
            await dbContext.SaveChangesAsync();

            return item;
        }
    }
}
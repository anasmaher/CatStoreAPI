using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WishListRepository : Repository<WishList>, IWishListRepository
    {
        private readonly AppDbContext dbContext;

        public WishListRepository(AppDbContext _dbContext) : base(_dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task<Product> AddWishlistProductAsync(int wishlistId, int productId)
        {
            var wishlist = await GetWishListWithProductsAsync(wishlistId);

            var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

            wishlist.Products.Add(product);

            return product;
        }

        public async Task<WishList> CreateWishListAsync()
        {
            var wishlist = new WishList();
            await dbContext.WishLists.AddAsync(wishlist);

            return wishlist;
        }

        public async Task<WishList> GetWishListWithProductsAsync(int id)
        {
            var wishlist = await dbContext.WishLists
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);

            return wishlist;
        }

        public async Task<Product> RemoveWishListItemAsync(int wishlistId, int productId)
        {
            var wishlist = await GetWishListWithProductsAsync(wishlistId);
            var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

            wishlist.Products.Remove(product);

            return product;
        }
    }
}

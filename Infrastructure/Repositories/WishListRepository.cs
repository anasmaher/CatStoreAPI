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

        public async Task<WishList> CreateWishListAsync(string userId)
        {
            var list = new WishList
            {
                userId = userId
            };

            await dbContext.WishLists.AddAsync(list);
            await dbContext.SaveChangesAsync();

            return list;
        }

        public async Task<WishList> GetWishListWithProductsAsync(string userId)
        {
            var list = await dbContext.WishLists
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.userId == userId);

            if (list is null)
                list = await CreateWishListAsync(userId);

            return list;
        }

        public async Task<Product> AddWishlistProductAsync(string userId, int productId)
        {

            var list = await GetWishListWithProductsAsync(userId);
            if (list is null)
            {
                list = await CreateWishListAsync(userId);
            }

            var existingListProduct = list.Products.FirstOrDefault(x => x.Id == productId);

            if (existingListProduct is not null) return existingListProduct;
            
            var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
            list.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return product;
        }

        public async Task<bool> RemoveWishListProductAsync(string userId, int productId)
        {
            var list = await GetWishListWithProductsAsync(userId);
            var product = list.Products.FirstOrDefault(x => x.Id == productId);

            if (product is null)
                return false;

            list.Products.Remove(product);

            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}
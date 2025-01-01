using CatStoreAPI.Core.Models;
using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext dbContext;

        public ICategoryRepository Categories { get; private set; }
        public IProductRepository Products { get; private set; }
        public IShoppingCartRepository ShoppingCarts { get; private set; }
        public IWishListRepository WishLists { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public IOrderRepository Orders { get; private set; }

        public UnitOfWork(AppDbContext _dbContext, ICategoryRepository _categoryRepo, IProductRepository _productRepo, IShoppingCartRepository _shoppingCartRepo, IWishListRepository _wishlistRepo, IReviewRepository _reviewRepo, IOrderRepository orders)
        {
            dbContext = _dbContext;
            Categories = _categoryRepo;
            Products = _productRepo;
            ShoppingCarts = _shoppingCartRepo;
            WishLists = _wishlistRepo;
            Reviews = _reviewRepo;
            Orders = orders;
        }

        public async Task<Product> AddProductWithNewCategoryAsync(Product product, string categoryName)
        {
            var existingCategory = await Categories.GetSingleAsync(c => c.Name == categoryName);

            if (existingCategory is null)
            {
                var category = new Category { Name = categoryName };
                category.DisplayOrder = dbContext.Categories.Max(x => x.DisplayOrder);

                await Categories.AddAsync(category);

                product.CategoryId = category.Id;
                product.Category = category;
            }
            else
            {
                product.CategoryId = existingCategory.Id;
                product.Category = existingCategory;
            }

            await Products.AddAsync(product);
            return product;
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}

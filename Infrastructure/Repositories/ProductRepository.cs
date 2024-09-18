using CatStoreAPI.Core.Models;
using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;

namespace Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext dbContext;
        private readonly ICategoryRepository categoryRepo;

        public ProductRepository(AppDbContext _dbContext, ICategoryRepository _categoryRepo) : base(_dbContext)
        {
            dbContext = _dbContext;
            categoryRepo = _categoryRepo;
        }

        public async Task<IEnumerable<Product>> SetOfferOnBrandProducts(string BrandName, int Discount)
        {
            var products = await GetAllAsync(x => x.Brand == BrandName);

            foreach (var product in products)
            {
                product.Discount = Discount;
            }

            return products;
        }

        public async Task<IEnumerable<Product>> SetOfferOnCategoryProducts(string CategoryName, int Discount)
        {
            var products = await GetAllAsync(x => x.Category.Name == CategoryName);

            foreach (var product in products)
            {
                product.Discount = Discount;
            }

            return products;
        }

        public async Task<IEnumerable<Product>> SetOfferOnMultipleProducts(List<int> Ids, int Discount)
        {
            var products = new List<Product>();

            foreach (var id in Ids)
            {
                var product = await GetSingleAsync(x => x.Id == id);
                product.Discount = Discount;

                products.Add(product);
            }

            return products;
        }

        public async Task<Product> SetOfferOnSingleProduct(int Id, int Discount)
        {
            var product = await GetSingleAsync(x => x.Id == Id);

            if (product is not null)
            {
                product.Discount = Discount;

                return product;
            }
            else
                throw new Exception("Product not found!");
        }

        public async Task<Product> UpdateProductAsync(int Id, Product product, string categoryName)
        {
            var UpdatedProduct = await GetSingleAsync(x => x.Id == Id);

            if (UpdatedProduct is not null)
            {
                UpdatedProduct.Name = product.Name;
                UpdatedProduct.Description = product.Description;
                UpdatedProduct.Price = product.Price;
                UpdatedProduct.Brand = product.Brand;
                UpdatedProduct.LifeStage = product.LifeStage;
                UpdatedProduct.ProductCode = product.ProductCode;
                UpdatedProduct.ImageUrl = product.ImageUrl;
                var cat= await categoryRepo.GetSingleAsync(x => x.Name == categoryName);
                UpdatedProduct.CategoryId = cat.Id;

                dbContext.Update(UpdatedProduct);

                return UpdatedProduct;
            }
            else
            {
                throw new Exception("Product not found!");
            }
        }
    }
}

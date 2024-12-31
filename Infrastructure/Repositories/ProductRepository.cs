using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<Product>> GetAllAsync(string searchName = null, string searchCategory = null, string searchBrand = null, string sortBy = null, bool isSortAscending = true, int page = 1, int pageSize = 10)
        {
            var query = dbContext.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(sortBy))
            {
                if (isSortAscending)
                    query = query.OrderBy(p => EF.Property<object>(p, sortBy));
                else
                    query = query.OrderByDescending(p => EF.Property<object>(p, sortBy));
            }

            if (!string.IsNullOrEmpty(searchCategory))
            {
                query = query.Where(p => p.Category.Name.Contains(searchCategory));
            }

            if (!string.IsNullOrEmpty(searchBrand))
            {
                query = query.Where(p => p.Brand.Contains(searchBrand));
            }

            return await query
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SetOfferOnBrandProducts(string BrandName, int Discount)
        {
            var products = await GetAllAsync(searchBrand: BrandName);

            foreach (var product in products)
            {
                product.Discount = Discount;
            }

            return products;
        }

        public async Task<IEnumerable<Product>> SetOfferOnCategoryProducts(string CategoryName, int Discount)
        {
            var products = await GetAllAsync(searchCategory: CategoryName);

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

        public void UpdateProduct(Product product)
        {
            dbContext.Products.Update(product);
        }
    }
}

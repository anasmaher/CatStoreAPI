using CatStoreAPI.Core.Models;
using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext dbContext;
        private readonly ICategoryRepository categoryRepo;
        private readonly IProductRepository productRepo;

        public UnitOfWork(AppDbContext _dbContext, ICategoryRepository _categoryRepo, IProductRepository _productRepo)
        {
            dbContext = _dbContext;
            categoryRepo = _categoryRepo;
            productRepo = _productRepo;

            Categories = _categoryRepo;
            Products = _productRepo;
        }

        public ICategoryRepository Categories { get; private set; }
        public IProductRepository Products { get; private set; }

        public async Task<Product> AddProductWithNewCategoryAsync(Product product, string categoryName)
        {
            

            var category = new Category { Name = categoryName };
            await Categories.AddAsync(category);

            product.CategoryId = category.Id;
            await Products.AddAsync(product);

            await SaveAsync();

            return product;
        }

        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}

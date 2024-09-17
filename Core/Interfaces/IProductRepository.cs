using Core.Models;
using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        public Task<Product> UpdateProductAsync(int Id, Product obj);

        Task<Product> SetOfferOnSingleProduct(int Id, int Discount);

        Task<IEnumerable<Product>> SetOfferOnMultipleProducts(List<int> Ids, int Discount);

        Task<IEnumerable<Product>> SetOfferOnBrandProducts(string BrandName, int Discount);

        Task<IEnumerable<Product>> SetOfferOnCategoryProducts(string CategoryName, int Discount);
    
        
    }
}

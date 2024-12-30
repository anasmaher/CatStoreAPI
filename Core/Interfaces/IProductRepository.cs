using Core.Models;
using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetAllAsync(string searchName = null, string searchCategory = null, string searchBrand = null, string sortBy = null, bool isSortAscending = true, int page = 1, int pageSize = 10);

        Task<Product> UpdateProductAsync(int Id, Product obj, string categoryName);

        Task<Product> SetOfferOnSingleProduct(int Id, int Discount);

        Task<IEnumerable<Product>> SetOfferOnMultipleProducts(List<int> Ids, int Discount);

        Task<IEnumerable<Product>> SetOfferOnBrandProducts(string BrandName, int Discount);

        Task<IEnumerable<Product>> SetOfferOnCategoryProducts(string CategoryName, int Discount);
    
        
    }
}

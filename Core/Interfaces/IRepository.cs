using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null);

        Task<T> GetSingleAsync(Expression<Func<T, bool>> filter);

        Task<T> AddAsync(T Entity);

        Task RemoveAsync(Expression<Func<T, bool>> filter);

        Task SaveAsync();
    }
}

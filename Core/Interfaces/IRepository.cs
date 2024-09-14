using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null);

        Task<T> GetSingleAsync(Expression<Func<T, bool>> filter);

        Task AddAsync(T Entity);

        Task RemoveAsync(T Entity);

        Task RemoveRangeAsync(IEnumerable<T> entities);

        Task SaveAsync();
    }
}

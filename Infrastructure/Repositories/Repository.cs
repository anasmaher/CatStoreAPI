using Core.Interfaces;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext dbContext;
        private readonly DbSet<T> dbSet;

        public Repository(AppDbContext _dbContext)
        {
            this.dbContext = _dbContext;
            this.dbSet = dbContext.Set<T>();
        }

        public virtual async Task<T> AddAsync(T Entity)
        {
            await dbSet.AddAsync(Entity);

            return Entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = dbSet;

            if (filter is not null)
                query = query.Where(filter);
            
            return await query.ToListAsync();

        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = dbSet;

            var result = await query.FirstOrDefaultAsync(filter);

            return result;
        }

        public async Task RemoveAsync(Expression<Func<T, bool>> filter)
        {
            var res = await GetSingleAsync(filter);

            if (res is not null)
            {
                dbSet.Remove(res);
            }
            else
                throw new Exception("Not found!");

        }
    }
}

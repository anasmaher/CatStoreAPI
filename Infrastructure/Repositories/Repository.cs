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

        public async Task AddAsync(T Entity)
        {
            await dbSet.AddAsync(Entity);
            await SaveAsync();
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

            var result = await query.SingleOrDefaultAsync(filter);

            if (result is not null)
                return result;
            else
                throw new Exception("Not found!");
        }

        public async Task RemoveAsync(T Entity)
        {
            dbSet.Remove(Entity);

            await SaveAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);

            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}

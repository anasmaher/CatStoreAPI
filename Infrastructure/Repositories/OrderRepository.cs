using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;

namespace Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

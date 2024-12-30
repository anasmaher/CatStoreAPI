using Core.Models;

namespace Core.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<List<Review>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10, bool asc = false);

        Task<Review> GetReviewByIdAsync(int reviewId);

        Task<Review> GetUserReviewForProductAsync(string userId, int productId);

        Task<Review> UpdateReviewAsync(int id, Review review);
    }
}

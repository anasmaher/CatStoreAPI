using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        private readonly AppDbContext context;

        public ReviewRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10, bool asc = false)
        {
            var reviews = await context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            List<Review> pagedReviews = new List<Review>();
            if (!asc)
            {
                pagedReviews = reviews
                .OrderByDescending(r => r.Rating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToList();
            }
            else
            {
                pagedReviews = reviews
                .OrderBy(r => r.Rating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToList();
            }

            return pagedReviews;
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            return await context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<Review> GetUserReviewForProductAsync(string userId, int productId)
        {
            return await context.Reviews.FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task<Review> UpdateReviewAsync(int id, Review review)
        {
            var updatedReview = await context.Reviews.FirstOrDefaultAsync(x => x.Id == id);

            if (updatedReview is not null)
            {
                updatedReview.Rating = review.Rating;
                updatedReview.Comment = review.Comment;

                return updatedReview;
            }
            else
            {
                throw new Exception("Review not found!");
            }
        }
    }
}
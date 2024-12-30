using AutoMapper;
using CatStoreAPI.DTO.ReviewDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Net;
using System.Security.Claims;

namespace CatStoreAPI.Controllers
{
    [Route("api/products/{productId}/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly APIResponse response;

        public ReviewController(IReviewRepository reviewRepository, IProductRepository productRepository, IMapper mapper, IUnitOfWork unitOfWork, IOutputCacheStore outputCacheStore)
        {
            this.reviewRepository = reviewRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.outputCacheStore = outputCacheStore;
            response = new APIResponse();
        }

        // GET: api/products/{productId}/reviews
        [HttpGet]
        [OutputCache(Duration = 60, VaryByRouteValueNames = ["productId"], Tags = ["Reviews"])]
        public async Task<ActionResult<APIResponse>> GetReviews(int productId, int page = 1, int pageSize = 10, bool asc = false)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            try
            {
                var product = await unitOfWork.Products.GetSingleAsync(x => x.Id == productId);
                if (product is null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Product not found.");
                    return NotFound(response);
                }

                var reviews = await unitOfWork.Reviews.GetReviewsByProductIdAsync(productId, page, pageSize, asc);

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = reviews;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }

        // GET: api/products/{productId}/reviews/{id}
        [HttpGet("{id}")]
        [OutputCache(Duration = 120, VaryByRouteValueNames = ["productId", "id"], Tags = ["Review"])]
        public async Task<ActionResult<Review>> GetReview(int productId, int id)
        {
            var review = await unitOfWork.Reviews.GetReviewByIdAsync(id);

            if (review is null || review.ProductId != productId)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Review not found.");
                return NotFound(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = review;
            return Ok(response);
        }

        // POST: api/products/{productId}/reviews
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Review>> CreateReview(int productId, ReviewCreateDTO reviewCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var product = await unitOfWork.Products.GetSingleAsync(x => x.Id == productId);
            if (product is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Product not found.");
                return NotFound(response);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the user has already reviewed this product
            var existingReview = await unitOfWork.Reviews.GetUserReviewForProductAsync(userId, productId);
            if (existingReview is not null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("User already reviewed this product.");
                return BadRequest(response);
            }

            var review = mapper.Map<Review>(reviewCreateDTO);
            review.UserId = userId;
            
            product.Reviews.Add(review);
            
            await unitOfWork.Reviews.AddAsync(review);
            await unitOfWork.SaveChangesAsync();

            await outputCacheStore.EvictByTagAsync($"Reviews-{productId}", HttpContext.RequestAborted);
            await outputCacheStore.EvictByTagAsync($"Review-{productId}, {review.Id}", HttpContext.RequestAborted);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = review;
            return Ok(response);
        }

        // PUT: api/products/{productId}/reviews/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int productId, int id, ReviewEditDTO reviewEditDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            try
            {
                var review = await unitOfWork.Reviews.GetSingleAsync(x => x.Id == id);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (review.UserId != userId)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.Forbidden;
                    response.Errors.Add("User is not authorized to update this review.");
                    return BadRequest(response);
                }

                mapper.Map(reviewEditDTO, review);

                await unitOfWork.Reviews.UpdateReviewAsync(id, review);
                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync($"Reviews-{productId}", HttpContext.RequestAborted);
                await outputCacheStore.EvictByTagAsync($"Review-{productId}, {review.Id}", HttpContext.RequestAborted);

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Review not found.");
                return NotFound(response);
            }
        }

        // DELETE: api/products/{productId}/reviews/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int productId, int id)
        {
            var review = await unitOfWork.Reviews.GetReviewByIdAsync(id);
            if (review is null || review.ProductId != productId)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Review not found.");
                return NotFound(response);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != userId)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Forbidden;
                response.Errors.Add("User is not authorized to update this review.");
                return BadRequest(response);
            }

            await unitOfWork.Reviews.RemoveAsync(x => x.Id == id);
            await unitOfWork.SaveChangesAsync();

            await outputCacheStore.EvictByTagAsync($"Reviews-{productId}", HttpContext.RequestAborted);
            await outputCacheStore.EvictByTagAsync($"Review-{productId}, {review.Id}", HttpContext.RequestAborted);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }
    }
}

using AutoMapper;
using CatStoreAPI.DTO.ReviewDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Retrieves a paginated list of reviews for a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="page">Page number for pagination (default is 1).</param>
        /// <param name="pageSize">Number of items per page (default is 10).</param>
        /// <param name="asc">Sort order: true for ascending, false for descending (default is false).</param>
        /// <returns>An ActionResult containing an APIResponse with the list of reviews.</returns>
        /// <response code="200">Reviews retrieved successfully.</response>
        /// <response code="400">Bad request due to invalid parameters.</response>
        /// <response code="404">Product not found.</response>
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

        /// <summary>
        /// Retrieves a specific review by its unique identifier for a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="id">The unique identifier of the review.</param>
        /// <returns>An ActionResult containing an APIResponse with the review details.</returns>
        /// <response code="200">Review retrieved successfully.</response>
        /// <response code="404">Review not found.</response>
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

        /// <summary>
        /// Creates a new review for a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="reviewCreateDTO">An object containing the details of the review to create.</param>
        /// <returns>An ActionResult containing an APIResponse with the created review.</returns>
        /// <response code="200">Review created successfully.</response>
        /// <response code="400">Bad request due to validation errors or user already reviewed the product.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">Product not found.</response>
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
            response.StatusCode = HttpStatusCode.Created;
            response.Result = review;
            return Ok(response);
        }

        /// <summary>
        /// Updates an existing review for a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="id">The unique identifier of the review to update.</param>
        /// <param name="reviewEditDTO">An object containing the updated review details.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        /// <response code="200">Review updated successfully.</response>
        /// <response code="400">Bad request due to validation errors or unauthorized access.</response>
        /// <response code="403">User is forbidden from updating this review.</response>
        /// <response code="404">Review not found.</response>
        /// <response code="401">User is not authenticated.</response>
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
                    return Forbid(response.ToString());
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

        /// <summary>
        /// Deletes an existing review for a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="id">The unique identifier of the review to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        /// <response code="200">Review deleted successfully.</response>
        /// <response code="400">Bad request due to unauthorized access.</response>
        /// <response code="403">User is forbidden from deleting this review.</response>
        /// <response code="404">Review not found.</response>
        /// <response code="401">User is not authenticated.</response>
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
                response.Errors.Add("User is not authorized to delete this review.");
                return Forbid(response.ToString());
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
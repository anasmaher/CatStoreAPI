using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Net;
using System.Security.Claims;

namespace CatStoreAPI.Controllers
{
    /// <summary>
    /// Controller for managing users' wishlists.
    /// </summary>
    [Route("api/Wishlist")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly APIResponse response;

        public WishlistController(IUnitOfWork _unitOfWork, IOutputCacheStore outputCacheStore)
        {
            unitOfWork = _unitOfWork;
            this.outputCacheStore = outputCacheStore;
            this.response = new APIResponse();
        }

        /// <summary>
        /// Retrieves the current user's wishlist along with all products.
        /// </summary>
        /// <returns>An IActionResult containing an APIResponse with the user's wishlist.</returns>
        /// <response code="200">Wishlist retrieved successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpGet]
        [Authorize]
        [OutputCache(Duration = 60, Tags = ["WishList"])]
        public async Task<IActionResult> GetWishListWithProductsAsync()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var wishlist = await unitOfWork.WishLists.GetWishListWithProductsAsync(userId);

                response.Result = wishlist;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Adds a product to the current user's wishlist.
        /// </summary>
        /// <param name="productId">The unique identifier of the product to add.</param>
        /// <returns>An IActionResult containing an APIResponse with the added product details.</returns>
        /// <response code="200">Product added to wishlist successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpPost("AddWishlistProduct/{productId}")]
        [Authorize]
        public async Task<IActionResult> AddWishlistProductAsync(int productId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var product = await unitOfWork.WishLists.AddWishlistProductAsync(userId, productId);
                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync($"WishList", HttpContext.RequestAborted);

                response.Result = product;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Removes a product from the current user's wishlist.
        /// </summary>
        /// <param name="productId">The unique identifier of the product to remove.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        /// <response code="200">Product removed from wishlist successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpDelete("RemoveWishListProduct/{productId}")]
        [Authorize]
        public async Task<IActionResult> RemoveWishListProductAsync(int productId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await unitOfWork.WishLists.RemoveWishListProductAsync(userId, productId);
                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync($"WishList", HttpContext.RequestAborted);

                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }
    }
}
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
    /// Controller for managing users' shopping carts.
    /// </summary>
    [Route("api/ShoppingCart")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly APIResponse response;

        public ShoppingCartController(IUnitOfWork _unitOfWork, IOutputCacheStore outputCacheStore)
        {
            unitOfWork = _unitOfWork;
            this.outputCacheStore = outputCacheStore;
            this.response = new APIResponse();
        }

        /// <summary>
        /// Retrieves the current user's shopping cart along with all items.
        /// </summary>
        /// <returns>An ActionResult containing an APIResponse with the user's shopping cart.</returns>
        /// <response code="200">Shopping cart retrieved successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpGet]
        [Authorize]
        [OutputCache(Duration = 60, Tags = ["Cart"])]
        public async Task<ActionResult<APIResponse>> GetCartWithItemsAsync()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await unitOfWork.ShoppingCarts.GetCartWithItemsAsync(userId);

                response.Result = cart;
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
        /// Retrieves a specific item from the user's shopping cart by item ID.
        /// </summary>
        /// <param name="id">The unique identifier of the cart item.</param>
        /// <returns>An ActionResult containing an APIResponse with the cart item details.</returns>
        /// <response code="200">Cart item retrieved successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpGet("GetCartItemById/{id}")]
        [Authorize]
        [OutputCache(Duration = 120, VaryByRouteValueNames = ["id"], Tags = ["CartItem"])]
        public async Task<ActionResult<APIResponse>> GetCartItemByIdAsync(int id)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.GetCartItemByIdAsync(id);

                response.Result = item;
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
        /// Adds a new item to the user's shopping cart.
        /// </summary>
        /// <param name="ProductId">The unique identifier of the product to add.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        /// <returns>An ActionResult containing an APIResponse with the added cart item details.</returns>
        /// <response code="200">Item added to cart successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpPost("AddItem")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> AddItemAsync(int ProductId, int quantity)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var item = await unitOfWork.ShoppingCarts.AddItemAsync(userId, ProductId, quantity);

                await outputCacheStore.EvictByTagAsync("Cart", HttpContext.RequestAborted);

                response.Result = item;
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
        /// Updates the quantity of an existing item in the user's shopping cart.
        /// </summary>
        /// <param name="itemId">The unique identifier of the cart item to update.</param>
        /// <param name="quantity">The new quantity of the product.</param>
        /// <returns>An ActionResult containing an APIResponse with the updated cart item details.</returns>
        /// <response code="200">Cart item updated successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpPut("UpdateCartItem")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> UpdateCartItemAsync(int itemId, int quantity)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var item = await unitOfWork.ShoppingCarts.UpdateCartItemAsync(userId, itemId, quantity);

                await outputCacheStore.EvictByTagAsync("Cart", HttpContext.RequestAborted);
                await outputCacheStore.EvictByTagAsync($"CartItem-{itemId}", HttpContext.RequestAborted);

                response.Result = item;
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
        /// Removes an item from the user's shopping cart.
        /// </summary>
        /// <param name="itemId">The unique identifier of the cart item to remove.</param>
        /// <returns>An ActionResult containing an APIResponse indicating the result of the operation.</returns>
        /// <response code="200">Cart item removed successfully.</response>
        /// <response code="400">Bad request due to an error.</response>
        /// <remarks>Requires authentication.</remarks>
        [HttpDelete("RemoveCartItem")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> RemoveCartItemAsync(int itemId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var item = await unitOfWork.ShoppingCarts.RemoveCartItemAsync(userId, itemId);

                await outputCacheStore.EvictByTagAsync("Cart", HttpContext.RequestAborted);
                await outputCacheStore.EvictByTagAsync($"CartItem-{itemId}", HttpContext.RequestAborted);

                response.Result = item;
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
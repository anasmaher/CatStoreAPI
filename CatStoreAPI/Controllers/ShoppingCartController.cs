using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Net;

namespace CatStoreAPI.Controllers
{
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

        [HttpGet("{id}")]
        [OutputCache(Duration = 60, Tags = ["Cart"])]
        public async Task<ActionResult<APIResponse>> GetCartWithItemsAsync(int id)
        {
            try
            {
                var cart = await unitOfWork.ShoppingCarts.GetCartWithItemsAsync(id);

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

        [HttpGet("getItem/{id}")]
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

        [HttpPost("{cartId}")]
        public async Task<ActionResult<APIResponse>> AddItemAsync(int cartId, int ProductId, int quantity)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.AddItemAsync(cartId, ProductId, quantity);

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

        [HttpPut("{itemId}")]
        public async Task<ActionResult<APIResponse>> UpdateCartItemAsync(int itemId, int quantity)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.UpdateCartItemAsync(itemId, quantity);

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

        [HttpDelete("{itemId}")]
        public async Task<ActionResult<APIResponse>> RemoveCartItemAsync(int itemId)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.RemoveCartItemAsync(itemId);

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

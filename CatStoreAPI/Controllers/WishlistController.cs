using Azure;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Net;
using System.Security.Claims;

namespace CatStoreAPI.Controllers
{
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

        [HttpPost("RemoveWishListProduct/{productId}")]
        [Authorize]
        public async Task<IActionResult> RemoveWishListProductAsync(int productId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await unitOfWork.WishLists.RemoveWishListProductAsync(userId, productId);
                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync($"WhishList", HttpContext.RequestAborted);

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

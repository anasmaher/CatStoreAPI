using Azure;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CatStoreAPI.Controllers
{
    [Route("api/Wishlist")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly APIResponse response;

        public WishlistController(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
            this.response = new APIResponse();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWishListWithProductsAsync(int id)
        {
            try
            {
                var wishlist = await unitOfWork.WishLists.GetWishListWithProductsAsync(id);

                response.Result = wishlist;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                return BadRequest(response);
            }
        }

        [HttpPost("AddToWishlist/{wishlistId}")]
        public async Task<IActionResult> AddWishlistProductAsync(int wishlistId, int productId)
        {
            try
            {
                var product = await unitOfWork.WishLists.AddWishlistProductAsync(wishlistId, productId);
                await unitOfWork.SaveChangesAsync();

                response.Result = product;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                return BadRequest(response);
            }
        }

        [HttpPost("RemoveFromWishlist/{wishlistId}")]
        public async Task<IActionResult> RemoveWishListItemAsync(int wishlistId, int productId)
        {
            try
            {
                var product = await unitOfWork.WishLists.RemoveWishListItemAsync(wishlistId, productId);
                await unitOfWork.SaveChangesAsync();

                response.Result = product;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                return BadRequest(response);
            }
        }
    }
}

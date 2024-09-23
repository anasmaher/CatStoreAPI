using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatStoreAPI.Controllers
{
    [Route("api/Wishlist")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public WishlistController(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWishListWithProductsAsync(int id)
        {
            try
            {
                var wishlist = await unitOfWork.WishLists.GetWishListWithProductsAsync(id);

                return Ok(wishlist);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("AddToWishlist/{wishlistId}")]
        public async Task<IActionResult> AddWishlistProductAsync(int wishlistId, int productId)
        {
            try
            {
                var product = await unitOfWork.WishLists.AddWishlistProductAsync(wishlistId, productId);
                await unitOfWork.SaveChangesAsync();

                return Ok(product);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("RemoveFromWishlist/{wishlistId}")]
        public async Task<IActionResult> RemoveWishListItemAsync(int wishlistId, int productId)
        {
            try
            {
                var product = await unitOfWork.WishLists.RemoveWishListItemAsync(wishlistId, productId);
                await unitOfWork.SaveChangesAsync();

                return Ok(product);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}

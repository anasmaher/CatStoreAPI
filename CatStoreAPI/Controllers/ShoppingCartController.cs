using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public ShoppingCartController(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetCartWithItemsAsync(int id)
        {
            try
            {
                var cart = await unitOfWork.ShoppingCarts.GetCartWithItemsAsync(id);
                
                return Ok(cart);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("getItem/{id}")]
        public async Task<IActionResult> GetCartItemByIdAsync(int id)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.GetCartItemByIdAsync(id);
                
                return Ok(item);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddItemAsync(int cartId, int ProductId)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.AddItemAsync(cartId, ProductId);
                
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateCartItemAsync(int itemId, int quantity)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.UpdateCartItemAsync(itemId, quantity);

                return Ok(item);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveCartItemAsync(int itemId)
        {
            try
            {
                var item = await unitOfWork.ShoppingCarts.RemoveCartItemAsync(itemId);

                return Ok(item);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}

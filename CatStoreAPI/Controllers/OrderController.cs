using AutoMapper;
using CatStoreAPI.DTO.OrderDTOs;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace CatStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly APIResponse response;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPaymentService paymentService;
        private readonly IMapper mapper;

        public OrderController(IUnitOfWork unitOfWork, IPaymentService paymentService, IMapper mapper)
        {
            response = new APIResponse();
            this.unitOfWork = unitOfWork;
            this.paymentService = paymentService;
            this.mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<APIResponse>> CreateOrder(OrderCreateDTO createOrderDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            var cart = await unitOfWork.ShoppingCarts.GetSingleAsync(x => x.userId == userId);
            if (cart is null || !cart.Items.Any())
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Shopping cart is empty.");
                return BadRequest(response);
            }

            var paymentIntent = await paymentService.CreateOrUpdatePaymentIntent(userId);
            if (paymentIntent is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Payment processing failed.");
                return BadRequest(response);
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = cart.price,
                PaymentIntentId = paymentIntent.Id,
                ShippingAddress = mapper.Map<ShippingAddress>(createOrderDTO.ShippingAddress),
                OrderItems = cart.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.price,
                }).ToList(),
            };

            var productIds = order.OrderItems.Select(x => x.ProductId).ToList();
            List<Product> products = new List<Product>();
            foreach (var prodId  in productIds)
            {
                var product = await unitOfWork.Products.GetSingleAsync(x => x.Id == prodId);
                products.Add(product);
            }
            foreach (var orderItem in order.OrderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);
                if (product is null || product.StockAmount < orderItem.Quantity)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors.Add($"Product {product?.Name} is out of stock.");
                    return BadRequest(response);
                }
                product.StockAmount -= orderItem.Quantity;
            }

            await unitOfWork.Orders.AddAsync(order);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.ShoppingCarts.RemoveAsync(x => x.Id == cart.Id);
            await unitOfWork.SaveChangesAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = order;
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetOrdersForUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await unitOfWork.Orders.GetSingleAsync(x => x.UserId == userId);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = orders;
            return Ok(response);
        }
    }
}

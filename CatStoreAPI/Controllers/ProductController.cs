using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.ProductDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Net;

namespace CatStoreAPI.Controllers
{
    [Route("api/Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly APIResponse response;

        public ProductController(IMapper mapper, IUnitOfWork unitOfWork, IOutputCacheStore outputCacheStore)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.outputCacheStore = outputCacheStore;
            this.response = new APIResponse();
        }

        [HttpGet]
        [OutputCache(Duration = 60, Tags = ["Products"])]
        public async Task<ActionResult<APIResponse>> GetAllProducts(
            string searchName = null,
            string searchCategory = null,
            string searchBrand = null,
            string sortBy = null,
            bool isSortAscending = true,
            int page = 1,
            int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            try
            {
                var products = await unitOfWork.Products.GetAllAsync(searchName, searchCategory, searchBrand, sortBy, isSortAscending, page, pageSize);

                response.Result = products;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
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

        [HttpGet("{Id}")]
        [OutputCache(Duration = 120, VaryByRouteValueNames = ["Id"], Tags = ["Product"])]
        public async Task<ActionResult<APIResponse>> GetProductById(int Id)
        {
            try
            {
                var product = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> CreateProduct(ProductCreateDTO productDTO)
        {
            var existProductCode = await unitOfWork.Products.GetSingleAsync(x => x.ProductCode == productDTO.ProductCode);

            if (existProductCode is not null)
                ModelState.AddModelError("", "Product Code already exists.");

            if (ModelState.IsValid)
            {
                var createdProduct = mapper.Map<Product>(productDTO);

                await unitOfWork.AddProductWithNewCategoryAsync(createdProduct, productDTO.CategoryName);
                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);

                response.Result = createdProduct;
                response.StatusCode = HttpStatusCode.Created;
                response.IsSuccess = true;
                return Ok(response);
            }

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }

        [HttpPut("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> EditProduct(int Id, ProductUpdateDTO productDTO)
        {
            var existProduct = await unitOfWork.Products.GetSingleAsync(x => x.ProductCode == productDTO.ProductCode);

            if (existProduct is not null && existProduct.Id != Id)
                ModelState.AddModelError("", "Product Code already exists.");

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(productDTO, existProduct);
                    unitOfWork.Products.UpdateProduct(existProduct);
                    await unitOfWork.SaveChangesAsync();

                    await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);
                    await outputCacheStore.EvictByTagAsync($"Product-{Id}", HttpContext.RequestAborted);

                    response.Result = existProduct;
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.Errors.Add("Category not found!");
                    return BadRequest(response);
                }
            }

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }

        [HttpDelete("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> RemoveProduct(int Id)
        {
            try
            {
                var removedProduct = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);

                await unitOfWork.Products.RemoveAsync(x => x.Id == Id);
                await unitOfWork.SaveChangesAsync();

                await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);
                await outputCacheStore.EvictByTagAsync($"Product-{Id}", HttpContext.RequestAborted);

                response.Result = removedProduct;
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

        [HttpPost("ProductOfferSingle/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> SetOfferOnSingleProduct(int Id, int Discount)
        {
            var productOffer = await unitOfWork.Products.SetOfferOnSingleProduct(Id, Discount);
            await unitOfWork.SaveChangesAsync();

            await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);
            await outputCacheStore.EvictByTagAsync($"Product-{Id}", HttpContext.RequestAborted);

            response.Result = productOffer;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }

        [HttpPost("ProductOfferMultiple")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> SetOfferOnMultipleProducts(List<int> Ids, int Discount)
        {
            var productsOffer = await unitOfWork.Products.SetOfferOnMultipleProducts(Ids, Discount);
            await unitOfWork.SaveChangesAsync();

            await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);
            foreach (var item in productsOffer)
            {
                await outputCacheStore.EvictByTagAsync($"Product-{item.Id}", HttpContext.RequestAborted);
            }

            response.Result = productsOffer;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }

        [HttpPost("ProductOfferBrand")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> SetOfferOnBrandProducts(string BrandName, int Discount)
        {
            if (string.IsNullOrEmpty(BrandName))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Brand name cannot be null or empty.");
                return BadRequest(response);
            }

            var productsOffer = await unitOfWork.Products.SetOfferOnBrandProducts(BrandName, Discount);
            await unitOfWork.SaveChangesAsync();

            await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);
            foreach (var item in productsOffer)
            {
                await outputCacheStore.EvictByTagAsync($"Product-{item.Id}", HttpContext.RequestAborted);
            }

            response.Result = productsOffer;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }

        [HttpPost("ProductOfferCategory")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> SetOfferOnCategoriesProducts(string CategoryName, int Discount)
        {
            if (string.IsNullOrEmpty(CategoryName))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Category name cannot be null or empty.");
                return BadRequest(response);
            }

            var productsOffer = await unitOfWork.Products.SetOfferOnCategoryProducts(CategoryName, Discount);
            await unitOfWork.SaveChangesAsync();

            await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);
            foreach (var item in productsOffer)
            {
                await outputCacheStore.EvictByTagAsync($"Product-{item.Id}", HttpContext.RequestAborted);
            }

            response.Result = productsOffer;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }
    }
}

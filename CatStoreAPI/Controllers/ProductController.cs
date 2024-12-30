using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.ProductDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CatStoreAPI.Controllers
{
    [Route("api/Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly APIResponse response;

        public ProductController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.response = new APIResponse();
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllProducts(
            [FromQuery] string searchName = null,
            [FromQuery] string searchCategory = null,
            [FromQuery] string searchBrand = null,
            [FromQuery] string sortBy = null,
            [FromQuery] bool isSortAscending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
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

                response.Result = createdProduct;
                response.StatusCode = HttpStatusCode.Created;
                response.IsSuccess = true;
                return Ok(response);
            }
            return BadRequest(ModelState);
        }

        [HttpPut("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> EditProduct(int Id, ProductUpdateDTO productDTO)
        {
            var existProductCode = await unitOfWork.Products.GetSingleAsync(x => x.ProductCode == productDTO.ProductCode);

            if (existProductCode != null && existProductCode.Id != Id)
                ModelState.AddModelError("", "Product Code already exists.");

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedProduct = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);
                    await unitOfWork.Products.UpdateProductAsync(Id, updatedProduct, productDTO.CategoryName);

                    await unitOfWork.SaveChangesAsync();

                    response.Result = updatedProduct;
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
            return BadRequest(ModelState);
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

            response.Result = productsOffer;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }

        [HttpPost("ProductOfferBrand")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> SetOfferOnBrandProducts(string BrandName, int Discount)
        {
            var productsOffer = await unitOfWork.Products.SetOfferOnBrandProducts(BrandName, Discount);

            await unitOfWork.SaveChangesAsync();

            response.Result = productsOffer;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }

        [HttpPost("ProductOfferCategory")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> SetOfferOnCategoriesProducts(string CategoryName, int Discount)
        {
            var productsOffer = await unitOfWork.Products.SetOfferOnCategoryProducts(CategoryName, Discount);

            await unitOfWork.SaveChangesAsync();

            response.Result = productsOffer;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }
    }
}

using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.ProductDTOs;
using Core.Interfaces;
using Core.Models;
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
        public async Task<ActionResult<APIResponse>> GetAllProducts()
        {
            var products = await unitOfWork.Products.GetAllAsync();

            response.Result = products;
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
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
            else
                return BadRequest(ModelState);
        }

        [HttpPut("{Id}")]
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
            else
                return BadRequest(ModelState);
        }

        [HttpDelete("{Id}")]
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

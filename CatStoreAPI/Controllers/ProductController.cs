using AutoMapper;
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

        /// <summary>
        /// Retrieves a paginated list of products with optional filters and sorting.
        /// </summary>
        /// <param name="searchName">Filter by product name.</param>
        /// <param name="searchCategory">Filter by category name.</param>
        /// <param name="searchBrand">Filter by brand name.</param>
        /// <param name="sortBy">Field to sort by (e.g., "Price", "Name").</param>
        /// <param name="isSortAscending">Sort order: true for ascending, false for descending.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>An ActionResult containing an APIResponse with the list of products.</returns>
        /// <response code="200">Products retrieved successfully.</response>
        /// <response code="400">Bad request due to invalid parameters.</response>
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

        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="Id">The unique identifier of the product.</param>
        /// <returns>An ActionResult containing an APIResponse with the product details.</returns>
        /// <response code="200">Product retrieved successfully.</response>
        /// <response code="400">Bad request due to invalid product ID.</response>
        /// <response code="404">Product not found.</response>
        [HttpGet("{Id}")]
        [OutputCache(Duration = 120, VaryByRouteValueNames = ["Id"], Tags = ["Product"])]
        public async Task<ActionResult<APIResponse>> GetProductById(int Id)
        {
            try
            {
                var product = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);

                if (product is null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.Errors.Add("Product not found.");
                    return NotFound(response);
                }

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

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="productDTO">An object containing the details of the product to create.</param>
        /// <returns>An ActionResult containing an APIResponse with the created product.</returns>
        /// <response code="200">Product created successfully.</response>
        /// <response code="400">Bad request due to validation errors.</response>
        /// <remarks>Requires administrator privileges.</remarks>
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

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="Id">The unique identifier of the product to update.</param>
        /// <param name="productDTO">An object containing the updated product details.</param>
        /// <returns>An ActionResult containing an APIResponse with the updated product.</returns>
        /// <response code="200">Product updated successfully.</response>
        /// <response code="400">Bad request due to validation errors or invalid product code.</response>
        /// <response code="404">Product not found.</response>
        /// <remarks>Requires administrator privileges.</remarks>
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
                    var productToUpdate = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);

                    if (productToUpdate == null)
                    {
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.IsSuccess = false;
                        response.Errors.Add("Product not found.");
                        return NotFound(response);
                    }

                    mapper.Map(productDTO, productToUpdate);
                    unitOfWork.Products.UpdateProduct(productToUpdate);
                    await unitOfWork.SaveChangesAsync();

                    await outputCacheStore.EvictByTagAsync("Products", HttpContext.RequestAborted);
                    await outputCacheStore.EvictByTagAsync($"Product-{Id}", HttpContext.RequestAborted);

                    response.Result = productToUpdate;
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.Errors.Add("An error occurred while updating the product.");
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

        /// <summary>
        /// Deletes an existing product.
        /// </summary>
        /// <param name="Id">The unique identifier of the product to delete.</param>
        /// <returns>An ActionResult containing an APIResponse with details of the deleted product.</returns>
        /// <response code="200">Product deleted successfully.</response>
        /// <response code="400">Bad request due to invalid product ID.</response>
        /// <response code="404">Product not found.</response>
        /// <remarks>Requires administrator privileges.</remarks>
        [HttpDelete("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> RemoveProduct(int Id)
        {
            try
            {
                var removedProduct = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);

                if (removedProduct == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.Errors.Add("Product not found.");
                    return NotFound(response);
                }

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

        /// <summary>
        /// Applies a discount offer to a single product.
        /// </summary>
        /// <param name="Id">The unique identifier of the product.</param>
        /// <param name="Discount">The discount percentage to apply.</param>
        /// <returns>An ActionResult containing an APIResponse with the updated product.</returns>
        /// <response code="200">Discount applied successfully.</response>
        /// <response code="400">Bad request due to invalid parameters.</response>
        /// <remarks>Requires administrator privileges.</remarks>
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

        /// <summary>
        /// Applies a discount offer to multiple products.
        /// </summary>
        /// <param name="Ids">A list of product IDs to which the discount will be applied.</param>
        /// <param name="Discount">The discount percentage to apply.</param>
        /// <returns>An ActionResult containing an APIResponse with the updated products.</returns>
        /// <response code="200">Discount applied successfully to multiple products.</response>
        /// <response code="400">Bad request due to invalid parameters.</response>
        /// <remarks>Requires administrator privileges.</remarks>
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

        /// <summary>
        /// Applies a discount offer to all products of a specific brand.
        /// </summary>
        /// <param name="BrandName">The name of the brand.</param>
        /// <param name="Discount">The discount percentage to apply.</param>
        /// <returns>An ActionResult containing an APIResponse with the updated products.</returns>
        /// <response code="200">Discount applied successfully to brand products.</response>
        /// <response code="400">Bad request due to missing or invalid brand name.</response>
        /// <remarks>Requires administrator privileges.</remarks>
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

        /// <summary>
        /// Applies a discount offer to all products of a specific category.
        /// </summary>
        /// <param name="CategoryName">The name of the category.</param>
        /// <param name="Discount">The discount percentage to apply.</param>
        /// <returns>An ActionResult containing an APIResponse with the updated products.</returns>
        /// <response code="200">Discount applied successfully to category products.</response>
        /// <response code="400">Bad request due to missing or invalid category name.</response>
        /// <remarks>Requires administrator privileges.</remarks>
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
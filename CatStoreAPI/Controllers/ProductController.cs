using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.ProductDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatStoreAPI.Controllers
{
    [Route("api/Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public ProductController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await unitOfWork.Products.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetProductById(int Id)
        {
            try
            {
                var product = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductCreateDTO productDTO)
        {
            var existProductCode = await unitOfWork.Products.GetSingleAsync(x => x.ProductCode == productDTO.ProductCode);

            if (existProductCode is not null)
                ModelState.AddModelError("", "Product Code already exists.");

            if (ModelState.IsValid)
            {
                var createdProduct = mapper.Map<Product>(productDTO);

                await unitOfWork.AddProductWithNewCategoryAsync(createdProduct, productDTO.CategoryName);

                await unitOfWork.SaveChangesAsync();
                return Ok(createdProduct);
            }
            else
                return BadRequest(ModelState);
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> EditProduct(int Id, ProductUpdateDTO productDTO)
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
                    return Ok(updatedProduct);
                }
                catch (Exception ex)
                {
                    return NotFound("Category does not exist!");
                }
            }
            else
                return BadRequest(ModelState);
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> RemoveProduct(int Id)
        {
            try
            {
                var removedProduct = await unitOfWork.Products.GetSingleAsync(x => x.Id == Id);
                await unitOfWork.Products.RemoveAsync(x => x.Id == Id);

                await unitOfWork.SaveChangesAsync();
                return Ok(removedProduct);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("ProductOfferSingle/{Id}")]
        public async Task<IActionResult> SetOfferOnSingleProduct(int Id, int Discount)
        {
            var productOffer = await unitOfWork.Products.SetOfferOnSingleProduct(Id, Discount);
            
            await unitOfWork.SaveChangesAsync();
            return Ok(productOffer);
        }

        [HttpPost("ProductOfferMultiple")]
        public async Task<IActionResult> SetOfferOnMultipleProducts(List<int> Ids, int Discount)
        {
            var productsOffer = await unitOfWork.Products.SetOfferOnMultipleProducts(Ids, Discount);

            await unitOfWork.SaveChangesAsync();
            return Ok(productsOffer);
        }

        [HttpPost("ProductOfferBrand")]
        public async Task<IActionResult> SetOfferOnBrandProducts(string BrandName, int Discount)
        {
            var productsOffer = await unitOfWork.Products.SetOfferOnBrandProducts(BrandName, Discount);

            await unitOfWork.SaveChangesAsync();
            return Ok(productsOffer);
        }

        [HttpPost("ProductOfferCategory")]
        public async Task<IActionResult> SetOfferOnCategoriesProducts(string CategoryName, int Discount)
        {
            var productsOffer = await unitOfWork.Products.SetOfferOnCategoryProducts(CategoryName, Discount);

            await unitOfWork.SaveChangesAsync();
            return Ok(productsOffer);
        }
    }
}

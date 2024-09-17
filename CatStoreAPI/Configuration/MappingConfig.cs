using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.CategoryDTOs;
using CatStoreAPI.DTO.ProductDTOs;
using Core.Models;

namespace CatStoreAPI.Configuration
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Category, CategoryCreatDTO>().ReverseMap();
            CreateMap<Category, CategoryUpdateDTO>().ReverseMap();

            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Product, ProductCreateDTO>().ReverseMap();
            CreateMap<Product, ProductUpdateDTO>().ReverseMap();
        }
    }
}

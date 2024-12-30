using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO.AuthDTOs;
using CatStoreAPI.DTO.CategoryDTOs;
using CatStoreAPI.DTO.ProductDTOs;
using CatStoreAPI.DTO.ReviewDTOs;
using Core.Models;

namespace CatStoreAPI.Configuration
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<EditInfoDTO, AppUser>();

            CreateMap<Category, CategoryCreatDTO>().ReverseMap();
            CreateMap<Category, CategoryUpdateDTO>().ReverseMap();

            CreateMap<Product, ProductCreateDTO>().ReverseMap();
            CreateMap<Product, ProductUpdateDTO>().ReverseMap();

            CreateMap<ReviewCreateDTO, Review>().ReverseMap();
            CreateMap<ReviewEditDTO, Review>().ReverseMap();
        }
    }
}

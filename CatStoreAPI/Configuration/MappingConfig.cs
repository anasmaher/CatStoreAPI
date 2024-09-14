﻿using AutoMapper;
using CatStoreAPI.Core.Models;
using CatStoreAPI.DTO;

namespace CatStoreAPI.Configuration
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();
        }
    }
}

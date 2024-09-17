﻿using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.CategoryDTOs
{
    public class CategoryCreatDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage ="Display Order must be greater than 0.")]
        public int DisplayOrder { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}

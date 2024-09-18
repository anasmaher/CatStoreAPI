using Core.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CatStoreAPI.Core.Models
{
    public class Category
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public virtual List<Product> Products { get; set; }
    }
}

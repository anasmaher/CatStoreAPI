using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.CatStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public int OrderNumber { get; set; }

        public IFormFile? Image { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.ReviewDTOs
{
    public class ReviewCreateDTO
    {
        [Required]
        [Range(1, 5)]
        public int rating { get; set; }

        [Required, MaxLength(1000)]
        [DataType(DataType.MultilineText)]
        public string comment { get; set; }
    }
}

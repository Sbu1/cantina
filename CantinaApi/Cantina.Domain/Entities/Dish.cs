using System.ComponentModel.DataAnnotations;
namespace Cantina.Domain.Models
{
    public class Dish
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty ;

        [Required]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } = string.Empty;
    }
}

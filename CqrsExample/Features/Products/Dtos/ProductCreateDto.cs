using System.ComponentModel.DataAnnotations;

namespace CqrsExample.Dtos;

public class ProductCreateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(125, ErrorMessage = "Title must not exceed 125 characters.")]
    public string Title { get; set; }

    [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
    public decimal Price { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Please enter the product name")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Please enter a description")]
    public string Description { get; set; } // Dodano opis

    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; } // Dodano ilość w magazynie

    [Url(ErrorMessage = "Please enter a valid URL")]
    public string ImageUrl { get; set; } // Opcjonalny URL obrazka

    [ForeignKey("Category")]
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

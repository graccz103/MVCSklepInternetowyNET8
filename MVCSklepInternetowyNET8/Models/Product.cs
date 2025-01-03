using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
    public bool IsOnPromotion { get; set; }
    public DateTime? PromotionEndDate { get; set; }
    public DateTime CreatedDate { get; set; }

    // Przechowywanie dużego obrazu w formie binarnej
    public byte[] LargeImage { get; set; }

    // Przechowywanie miniaturki w formie binarnej
    public byte[] Thumbnail { get; set; }

    [ForeignKey("Category")]
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

using System.ComponentModel.DataAnnotations;

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; } // Cena pojedynczej sztuki w momencie zakupu

    public int OrderId { get; set; }
    public Order Order { get; set; }
}

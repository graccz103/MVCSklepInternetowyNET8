public class CartItem
{
    public int ProductId { get; set; } // Identyfikator produktu
    public string ProductName { get; set; } // Nazwa produktu
    public decimal Price { get; set; } // Cena produktu
    public int Quantity { get; set; } // Ilość
    public byte[] Thumbnail { get; set; } // Miniaturka produktu
}

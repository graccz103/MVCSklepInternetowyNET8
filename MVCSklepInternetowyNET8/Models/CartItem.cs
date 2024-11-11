public class CartItem
{
    public int ProductId { get; set; } // Identyfikator produktu
    public string? ProductName { get; set; } // Nazwa produktu
    public decimal Price { get; set; } // Cena produktu
    public int Quantity { get; set; } // Ilość w koszyku
    public byte[]? Thumbnail { get; set; } // Miniaturka produktu
    public int StockQuantity { get; set; } // Dostępna ilość w magazynie
}

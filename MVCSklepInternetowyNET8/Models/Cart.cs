public class Cart
{
    public int CartId { get; set; } // Unikalny identyfikator koszyka, wymagany, jeśli zapisujesz koszyk w bazie danych
    public string UserId { get; set; } // Identyfikator użytkownika, do którego należy koszyk

    public List<CartItem> Items { get; set; } = new List<CartItem>(); // Lista produktów w koszyku

    public decimal GetTotalPrice()
    {
        return Items.Sum(item => item.Price * item.Quantity);
    }

    public void AddToCart(Product product, int quantity)
    {
        var cartItem = Items.FirstOrDefault(i => i.ProductId == product.ProductId);
        if (cartItem == null)
        {
            Items.Add(new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                Thumbnail = product.Thumbnail
            });
        }
        else
        {
            cartItem.Quantity += quantity;
        }
    }

    public void RemoveFromCart(int productId)
    {
        var cartItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (cartItem != null)
        {
            Items.Remove(cartItem);
        }
    }
}

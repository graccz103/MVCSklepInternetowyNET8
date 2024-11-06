public class Cart
{
    public List<CartItem> Items { get; set; } = new List<CartItem>(); // Zmiana na List<CartItem>

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


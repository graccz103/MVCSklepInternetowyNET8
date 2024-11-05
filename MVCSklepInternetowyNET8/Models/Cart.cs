using System.Collections.Generic;
using System.Linq;

public class Cart
{
    private List<CartItem> items = new List<CartItem>();

    public IEnumerable<CartItem> Items => items;

    public void AddToCart(Product product, int quantity)
    {
        var item = items.FirstOrDefault(p => p.ProductId == product.ProductId);
        if (item == null)
        {
            items.Add(new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity += quantity;
        }
    }

    public void RemoveFromCart(int productId)
    {
        var item = items.FirstOrDefault(p => p.ProductId == productId);
        if (item != null)
        {
            items.Remove(item);
        }
    }

    public decimal GetTotalPrice()
    {
        return items.Sum(i => i.Price * i.Quantity);
    }

    public void ClearCart()
    {
        items.Clear();
    }
}

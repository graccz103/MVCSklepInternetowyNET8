// Services/CartService.cs
using Microsoft.AspNetCore.Http;
using MVCSklepInternetowyNET8.Models;
using Newtonsoft.Json;

public class CartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Cart GetCart()
    {
        var session = _httpContextAccessor.HttpContext.Session;
        var cartData = session.GetString("Cart");
        return string.IsNullOrEmpty(cartData) ? new Cart() : JsonConvert.DeserializeObject<Cart>(cartData);
    }

    public void SaveCart(Cart cart)
    {
        var session = _httpContextAccessor.HttpContext.Session;
        session.SetString("Cart", JsonConvert.SerializeObject(cart));
    }

    public void ClearCart()
    {
        SaveCart(new Cart());
    }
}

using Microsoft.AspNetCore.Mvc;
using MVCSklepInternetowyNET8.Models;
using Newtonsoft.Json;

public class CartController : Controller
{
    private readonly OnlineShopContext _context;

    public CartController(OnlineShopContext context)
    {
        _context = context;
    }

    private Cart GetCart()
    {
        var cart = HttpContext.Session.GetString("Cart");
        return cart == null ? new Cart() : JsonConvert.DeserializeObject<Cart>(cart);
    }

    private void SaveCart(Cart cart)
    {
        HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
    }

    // GET: Cart
    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }
    // POST: Cart/Add
    public IActionResult AddToCart(int productId)
    {
        var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
        if (product == null)
        {
            return NotFound();
        }

        var cart = GetCart();
        cart.AddToCart(product, 1); // Dodaj jeden produkt
        SaveCart(cart);

        TempData["SuccessMessage"] = "Produkt został dodany do koszyka.";
        return RedirectToAction("Index", "Cart"); // Zmiana przekierowania na widok koszyka
    }


    // POST: Cart/Remove
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = GetCart();
        cart.RemoveFromCart(productId);
        SaveCart(cart);

        TempData["SuccessMessage"] = "Produkt został usunięty z koszyka.";
        return RedirectToAction("Index");
    }

    // POST: Cart/Clear
    public IActionResult ClearCart()
    {
        var cart = new Cart();
        SaveCart(cart);

        TempData["SuccessMessage"] = "Koszyk został wyczyszczony.";
        return RedirectToAction("Index");
    }
}

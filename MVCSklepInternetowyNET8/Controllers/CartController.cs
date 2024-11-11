using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCSklepInternetowyNET8.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class CartController : Controller
{
    private readonly OnlineShopContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(OnlineShopContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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

    // GET: Cart/OrderConfirmation
    public async Task<IActionResult> OrderConfirmation(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }


    // POST: Cart/Add
    [HttpPost]
    public IActionResult AddToCart(int productId, int quantity)
    {
        var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
        if (product == null || quantity < 1 || quantity > product.StockQuantity)
        {
            TempData["ErrorMessage"] = "Nieprawidłowa ilość lub produkt niedostępny.";
            return RedirectToAction("ProductList", "Product");
        }

        var cart = GetCart();
        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (cartItem == null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                Thumbnail = product.Thumbnail,
                StockQuantity = product.StockQuantity // Ustawienie StockQuantity
            });
        }
        else
        {
            cartItem.Quantity += quantity;
        }

        SaveCart(cart);
        TempData["SuccessMessage"] = "Produkt został dodany do koszyka.";
        return RedirectToAction("Index", "Cart");
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

    // POST: Cart/Checkout
    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        // Logowanie w celu weryfikacji
        Console.WriteLine("Rozpoczęto proces Checkout");

        // Pobranie użytkownika bez sprawdzania danych klienta
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            Console.WriteLine("Użytkownik niezalogowany - przekierowanie do logowania.");
            return RedirectToAction("Login", "Account");
        }

        Console.WriteLine($"Użytkownik: {user.Email}");

        // Pobranie koszyka i sprawdzenie, czy są w nim produkty
        var cart = GetCart();
        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Twój koszyk jest pusty.";
            return RedirectToAction("Index");
        }

        Console.WriteLine("Koszyk zawiera produkty, przechodzimy do tworzenia zamówienia");

        // Tworzenie zamówienia
        var order = new Order
        {
            UserId = user.Id,
            TotalPrice = cart.GetTotalPrice(),
            OrderItems = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        Console.WriteLine("Zamówienie zostało pomyślnie złożone");

        ClearCart(); // Wyczyść koszyk po złożeniu zamówienia
        TempData["SuccessMessage"] = "Twoje zamówienie zostało złożone!";
        return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        var cart = GetCart();
        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (cartItem != null && quantity > 0)
        {
            cartItem.Quantity = quantity;
            SaveCart(cart);
            TempData["SuccessMessage"] = "Ilość produktu została zaktualizowana.";
        }
        else
        {
            TempData["ErrorMessage"] = "Nieprawidłowa ilość.";
        }

        return RedirectToAction("Index");
    }

}

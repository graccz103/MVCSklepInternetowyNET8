using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using MVCSklepInternetowyNET8.Models;

public class OrderController : Controller
{
    private readonly OnlineShopContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CartService _cartService;

    public OrderController(OnlineShopContext context, UserManager<ApplicationUser> userManager, CartService cartService)
    {
        _context = context;
        _userManager = userManager;
        _cartService = cartService;
    }

    // GET: Order/Checkout
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var user = await _context.Users
                                 .Include(u => u.Customer)
                                 .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

        // Sprawdzenie, czy użytkownik jest zalogowany i ma przypisane dane klienta z wymaganymi informacjami
        if (user == null || user.Customer == null ||
            string.IsNullOrWhiteSpace(user.Customer.FirstName) ||
            string.IsNullOrWhiteSpace(user.Customer.LastName) ||
            string.IsNullOrWhiteSpace(user.Customer.Address) ||
            string.IsNullOrWhiteSpace(user.Customer.City) ||
            string.IsNullOrWhiteSpace(user.Customer.PostalCode))
        {
            TempData["ErrorMessage"] = "Proszę uzupełnić wszystkie szczegóły konta przed złożeniem zamówienia.";
            return RedirectToAction("EditCustomerDetails", "Account");
        }

        var cart = _cartService.GetCart();

        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Twój koszyk jest pusty.";
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.CustomerDetails = user.Customer;
        return View(cart);
    }


    // POST: Order/Checkout
    [HttpPost]
    public async Task<IActionResult> CheckoutPost()
    {
        Console.WriteLine("Przetwarzanie zamówienia...");

        var user = await _context.Users
                                 .Include(u => u.Customer)
                                 .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

        // Sprawdzenie, czy użytkownik jest zalogowany i posiada uzupełnione wymagane szczegóły
        if (user == null || user.Customer == null ||
            string.IsNullOrWhiteSpace(user.Customer.FirstName) ||
            string.IsNullOrWhiteSpace(user.Customer.LastName) ||
            string.IsNullOrWhiteSpace(user.Customer.Address) ||
            string.IsNullOrWhiteSpace(user.Customer.City) ||
            string.IsNullOrWhiteSpace(user.Customer.PostalCode))
        {
            TempData["ErrorMessage"] = "Proszę uzupełnić wszystkie szczegóły konta przed złożeniem zamówienia.";
            return RedirectToAction("EditCustomerDetails", "Account");
        }

        var cart = _cartService.GetCart();

        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Twój koszyk jest pusty.";
            return RedirectToAction("Index", "Cart");
        }

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

        // Zmniejszanie `StockQuantity` dla każdego produktu dopiero po złożeniu zamówienia
        foreach (var item in cart.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity -= item.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        _cartService.ClearCart();
        TempData["SuccessMessage"] = "Twoje zamówienie zostało złożone!";
        return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
    }







    // GET: Order/OrderConfirmation
    [HttpGet]
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
}

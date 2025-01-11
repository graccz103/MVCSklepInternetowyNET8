using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using MVCSklepInternetowyNET8.Models;
using Microsoft.AspNetCore.Authorization;

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

        // Pobierz koszyk dla zalogowanego użytkownika
        var cart = await _cartService.GetCartAsync();

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

        // Pobierz koszyk dla zalogowanego użytkownika
        var cart = await _cartService.GetCartAsync();

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

        // Wyczyść koszyk użytkownika po złożeniu zamówienia
        await _cartService.ClearCartAsync(user.Id);
        TempData["SuccessMessage"] = "Twoje zamówienie zostało złożone!";
        return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
    }


    // GET: Order/ManageOrders
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.User) // Pobierz użytkowników dla zamówień
            .ToListAsync();

        // Ustaw domyślny status dla zamówień bez statusu
        foreach (var order in orders.Where(o => string.IsNullOrEmpty(o.OrderStatus)))
        {
            order.OrderStatus = "Nowe";
        }

        var groupedOrders = new
        {
            NewOrders = orders.Where(o => o.OrderStatus == "Nowe").OrderByDescending(o => o.OrderDate),
            InProgressOrders = orders.Where(o => o.OrderStatus == "W trakcie realizacji").OrderByDescending(o => o.OrderDate),
            CompletedOrders = orders.Where(o => o.OrderStatus == "Zrealizowane").OrderByDescending(o => o.OrderDate),
            CancelledOrders = orders.Where(o => o.OrderStatus == "Anulowane").OrderByDescending(o => o.OrderDate)
        };

        return View(groupedOrders);
    }

    // POST: Order/ChangeOrderStatus
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeOrderStatus(int orderId, string newStatus)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono zamówienia.";
            return RedirectToAction("ManageOrders");
        }

        if (!new[] { "Nowe", "W trakcie realizacji", "Zrealizowane", "Anulowane" }.Contains(newStatus))
        {
            TempData["ErrorMessage"] = "Nieprawidłowy stan zamówienia.";
            return RedirectToAction("ManageOrders");
        }

        order.OrderStatus = newStatus;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Stan zamówienia został zaktualizowany.";
        return RedirectToAction("ManageOrders");
    }



    // GET: Order/OrderHistory
    [HttpGet]
    public async Task<IActionResult> OrderHistory()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Proszę się zalogować, aby zobaczyć historię zamówień.";
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();

        return View(orders);
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


    // POST: Order/RepeatOrder
    [HttpPost]
    public async Task<IActionResult> RepeatOrder(int orderId)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby ponowić zamówienie.";
            return RedirectToAction("Login", "Account");
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono zamówienia.";
            return RedirectToAction("OrderHistory");
        }

        var cart = await _cartService.GetCartAsync();

        foreach (var item in order.OrderItems)
        {
            var existingCartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == item.ProductId);

            if (existingCartItem == null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Thumbnail = item.Product.Thumbnail,
                    StockQuantity = item.Product.StockQuantity
                });
            }
            else
            {
                existingCartItem.Quantity += item.Quantity;
            }
        }

        await _cartService.SaveCartAsync(cart);

        TempData["SuccessMessage"] = "Produkty z zamówienia zostały dodane do koszyka.";
        return RedirectToAction("Index", "Cart");
    }

}



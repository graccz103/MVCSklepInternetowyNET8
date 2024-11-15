// Services/CartService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCSklepInternetowyNET8.Models;
using System.Threading.Tasks;

public class CartService
{
    private readonly OnlineShopContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartService(OnlineShopContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<Cart> GetCartAsync()
    {
        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);

        if (string.IsNullOrEmpty(userId))
        {
            return new Cart(); // pusty koszyk dla użytkowników niezalogowanych
        }

        var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    public async Task SaveCartAsync(Cart cart)
    {
        await _context.SaveChangesAsync();
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetCartAsync();
        cart.Items.Clear();
        await SaveCartAsync(cart);
    }
}

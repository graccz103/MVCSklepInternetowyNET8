using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MVCSklepInternetowyNET8.Models;
using Microsoft.AspNetCore.Authorization;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly OnlineShopContext _context;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, OnlineShopContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    // Akcja rejestracji
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }
        return View(model);
    }

    // Akcja logowania
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                // Rejestruj wizytę
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var visit = new Visit
                    {
                        VisitDate = DateTime.Now,
                        UserId = user.Id
                    };
                    _context.Visits.Add(visit);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Nieprawidłowe dane logowania.");
        }
        return View(model);
    }

    // Akcja wylogowania
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // Lista użytkowników
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListUsers()
    {
        var users = await _context.Users
            .Include(u => u.Customer)
            .ToListAsync();
        return View(users);
    }

    // POST: Usuwanie użytkownika
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono użytkownika.";
            return RedirectToAction("ListUsers");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Użytkownik został pomyślnie usunięty.";
        }
        else
        {
            TempData["ErrorMessage"] = "Wystąpił błąd podczas usuwania użytkownika.";
        }

        return RedirectToAction("ListUsers");
    }


    // GET: Account/CustomerDetails
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> CustomerDetails()
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["ErrorMessage"] = "Sesja wygasła. Proszę się zalogować ponownie.";
            return RedirectToAction("Login");
        }

        // Pobierz użytkownika wraz z danymi klienta
        var user = await _context.Users
                                 .Include(u => u.Customer)
                                 .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

        if (user == null)
        {
            TempData["ErrorMessage"] = "Nie jesteś zalogowany.";
            return RedirectToAction("Login");
        }

        if (user.Customer == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono danych klienta.";
            return RedirectToAction("EditCustomerDetails");
        }

        return View(user.Customer);
    }




    // GET: Account/EditCustomerDetails
    [HttpGet]
    public async Task<IActionResult> EditCustomerDetails()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Użytkownik nie jest zalogowany.";
            return RedirectToAction("Login");
        }

        var customerDetails = user.Customer ?? new Customer();
        return View(customerDetails);
    }

    // POST: Account/EditCustomerDetails
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCustomerDetails(Customer customer)
    {
        if (!ModelState.IsValid)
        {
            // Zbierz błędy walidacji i przekaż do widoku
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            ViewData["ModelErrors"] = errors;

            TempData["ErrorMessage"] = "Wystąpiły błędy w formularzu. Proszę sprawdzić dane.";
            return View(customer);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Użytkownik nie jest zalogowany.";
            return RedirectToAction("Login");
        }

        // Aktualizacja danych klienta
        if (user.Customer == null)
        {
            user.Customer = customer;
            user.Customer.UserId = user.Id; // Przypisanie UserId
            _context.Customers.Add(customer);
        }
        else
        {
            user.Customer.FirstName = customer.FirstName;
            user.Customer.LastName = customer.LastName;
            user.Customer.Email = customer.Email;
            user.Customer.PhoneNumber = customer.PhoneNumber;
            user.Customer.Address = customer.Address;
            user.Customer.City = customer.City;
            user.Customer.PostalCode = customer.PostalCode;
            user.Customer.UserId = user.Id; // Przypisanie UserId
            _context.Customers.Update(user.Customer);
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Dane zostały pomyślnie zaktualizowane.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Wystąpił błąd podczas zapisu: {ex.Message}";
        }

        return RedirectToAction("EditCustomerDetails");
    }
}

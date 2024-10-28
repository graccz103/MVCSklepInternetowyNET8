using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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
                Console.WriteLine("User registered successfully.");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Console.WriteLine("User registration failed.");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
            }
        }
        else
        {
            Console.WriteLine("Model state is invalid.");
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
                return RedirectToAction("Index", "Home");
            ModelState.AddModelError("", "Invalid login attempt.");
        }
        return View(model);
    }
    public async Task<IActionResult> ListUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            Console.WriteLine($"User: {user.Email}");
        }
        return View(users);
    }


    // Akcja wylogowania
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}

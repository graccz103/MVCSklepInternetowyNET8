using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja bazy danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OnlineShopContext>(options => options.UseSqlServer(connectionString));

// Konfiguracja ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<OnlineShopContext>()
    .AddDefaultTokenProviders();

// Konfiguracja autoryzacji i uwierzytelniania
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Dodanie kontrolerów i widoków
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Konfiguracja HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Domyœlnie HSTS na 30 dni
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Dodanie uwierzytelniania i autoryzacji
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

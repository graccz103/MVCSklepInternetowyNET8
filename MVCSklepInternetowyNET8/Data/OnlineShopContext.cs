using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class OnlineShopContext : IdentityDbContext<ApplicationUser>
{
    public OnlineShopContext(DbContextOptions<OnlineShopContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Wywołanie bazy IdentityDbContext do poprawnej konfiguracji kluczy Identity
        base.OnModelCreating(modelBuilder);

        // Ustawienie typu kolumny decimal dla właściwości TotalPrice w Order
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)");

        // Ustawienie typu kolumny decimal dla właściwości Price w OrderItem
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasColumnType("decimal(18,2)");

        // Ustawienie typu kolumny decimal dla właściwości Price w Product
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        // Ustawienie typu kolumny decimal dla właściwości Price w CartItem
        modelBuilder.Entity<CartItem>()
            .Property(c => c.Price)
            .HasColumnType("decimal(18,2)");

    // Konfiguracja relacji dla kategorii
    modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Subcategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Konfiguracja relacji jeden-do-jednego między ApplicationUser a Customer
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(a => a.Customer)
            .WithOne(c => c.User)
            .HasForeignKey<Customer>(c => c.UserId);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

public class OnlineShopContextFactory : IDesignTimeDbContextFactory<OnlineShopContext>
{
    public OnlineShopContext CreateDbContext(string[] args)
    {
        // Konfiguracja połączenia z appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<OnlineShopContext>();
        var connectionString = config.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new OnlineShopContext(optionsBuilder.Options);
    }
}

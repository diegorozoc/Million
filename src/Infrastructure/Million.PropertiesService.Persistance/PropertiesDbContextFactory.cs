using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Million.PropertiesService.Persistance;

public class PropertiesDbContextFactory : IDesignTimeDbContextFactory<PropertiesDbContext>
{
    public PropertiesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PropertiesDbContext>();
        
        // Build configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string from configuration with fallback options
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MillionPropertiesDb;Trusted_Connection=true;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString);

        return new PropertiesDbContext(optionsBuilder.Options);
    }
}
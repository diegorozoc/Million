using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Million.PropertiesService.Persistance;

namespace Million.PropertiesService.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test environment first
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove all database-related service registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<PropertiesDbContext>) ||
                d.ServiceType == typeof(PropertiesDbContext) ||
                d.ServiceType.Name.Contains("DbContext") ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Clear any connection string configurations that might interfere
            services.PostConfigure<DbContextOptions<PropertiesDbContext>>(options => { });

            // Add InMemory database with fresh configuration
            services.AddDbContext<PropertiesDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase(DatabaseName);
                options.EnableSensitiveDataLogging();
                options.UseInternalServiceProvider(null); // Don't use internal service provider to avoid conflicts
            }, ServiceLifetime.Scoped);
        });
    }

    public PropertiesDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
    }
}
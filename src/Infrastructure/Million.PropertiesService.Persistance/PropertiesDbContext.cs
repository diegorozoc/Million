using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;

namespace Million.PropertiesService.Persistance;

public class PropertiesDbContext : DbContext
{
    public PropertiesDbContext(DbContextOptions<PropertiesDbContext> options) : base(options)
    {
        
    }

    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
    public DbSet<PropertyTrace> PropertyTraces { get; set; } = null!;
    public DbSet<Owner> Owners { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
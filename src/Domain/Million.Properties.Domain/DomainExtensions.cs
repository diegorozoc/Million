using Microsoft.Extensions.DependencyInjection;
using Million.PropertiesService.Domain.Properties.Services;

namespace Million.PropertiesService.Domain;

public static class DomainExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Register domain services
        services.AddScoped<IPropertyOwnershipService, PropertyOwnershipService>();
        services.AddScoped<IPropertyValidationService, PropertyValidationService>();
        
        return services;
    }
}
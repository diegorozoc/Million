using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain;
using System.Reflection;

namespace Million.PropertiesServices.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register Domain Event services
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // Register all domain event handlers
        RegisterDomainEventHandlers(services, assembly);

        // Register domain services
        services.AddDomain();

        return services;
    }

    private static void RegisterDomainEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var domainEventHandlerTypes = assembly.GetTypes()
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
            .Where(type => !type.IsAbstract && !type.IsInterface);

        foreach (var handlerType in domainEventHandlerTypes)
        {
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, handlerType);
            }
        }
    }
}
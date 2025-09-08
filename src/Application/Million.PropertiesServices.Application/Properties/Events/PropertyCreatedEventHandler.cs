using Microsoft.Extensions.Logging;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain.Properties.Events;

namespace Million.PropertiesServices.Application.Properties.Events;

public sealed class PropertyCreatedEventHandler : IDomainEventHandler<PropertyCreated>
{
    private readonly ILogger<PropertyCreatedEventHandler> _logger;

    public PropertyCreatedEventHandler(ILogger<PropertyCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PropertyCreated domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Property created: {PropertyId} - {PropertyName} at {Address} for {Price}",
            domainEvent.PropertyId,
            domainEvent.Name,
            domainEvent.Address.GetFullAddress(),
            domainEvent.Price.ToString());

        // Here you could add additional side effects like:
        // - Sending notifications
        // - Updating read models
        // - Triggering external integrations
        // - Audit logging
        // - Analytics tracking

        return Task.CompletedTask;
    }
}
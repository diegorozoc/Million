using Microsoft.Extensions.Logging;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain.Properties.Events;

namespace Million.PropertiesServices.Application.Properties.Events;

public sealed class PropertyPriceChangedEventHandler : IDomainEventHandler<PropertyPriceChanged>
{
    private readonly ILogger<PropertyPriceChangedEventHandler> _logger;

    public PropertyPriceChangedEventHandler(ILogger<PropertyPriceChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PropertyPriceChanged domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Property price changed: {PropertyId} - New price: {NewPrice}",
            domainEvent.PropertyId,
            domainEvent.NewPrice.ToString());

        // Here you could add additional side effects like:
        // - Creating property trace records
        // - Notifying interested parties of price changes
        // - Updating property valuation models
        // - Triggering market analysis
        // - Sending alerts to subscribers

        return Task.CompletedTask;
    }
}
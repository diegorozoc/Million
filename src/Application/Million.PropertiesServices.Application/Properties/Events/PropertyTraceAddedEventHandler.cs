using Microsoft.Extensions.Logging;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain.Properties.Events;

namespace Million.PropertiesServices.Application.Properties.Events;

public sealed class PropertyTraceAddedEventHandler : IDomainEventHandler<PropertyTraceAdded>
{
    private readonly ILogger<PropertyTraceAddedEventHandler> _logger;

    public PropertyTraceAddedEventHandler(ILogger<PropertyTraceAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PropertyTraceAdded domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing PropertyTraceAdded event for Property {PropertyId}, TraceValue: {TraceValue}",
            domainEvent.PropertyId, domainEvent.TraceValue);

        try
        {
            // Property trace added event processed successfully
            _logger.LogInformation("Successfully processed PropertyTraceAdded event for Property {PropertyId} with TraceValue {TraceValue}",
                domainEvent.PropertyId, domainEvent.TraceValue);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PropertyTraceAdded event for Property {PropertyId}", 
                domainEvent.PropertyId);
            throw;
        }
    }
}
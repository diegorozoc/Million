using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Million.PropertiesService.Domain.Common.Events;

namespace Million.PropertiesServices.Application.Common.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dispatching domain event {EventType} with ID {EventId}", 
            domainEvent.GetType().Name, domainEvent.Id);

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        var tasks = new List<Task>();
        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle));
            if (handleMethod != null)
            {
                var task = (Task)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                tasks.Add(task);
            }
        }

        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("Successfully dispatched domain event {EventType} to {HandlerCount} handlers", 
                domainEvent.GetType().Name, tasks.Count);
        }
        else
        {
            _logger.LogWarning("No handlers found for domain event {EventType}", domainEvent.GetType().Name);
        }
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var events = domainEvents.ToList();
        if (!events.Any())
        {
            return;
        }

        _logger.LogInformation("Dispatching {EventCount} domain events", events.Count);

        var tasks = events.Select(domainEvent => DispatchAsync(domainEvent, cancellationToken));
        await Task.WhenAll(tasks);

        _logger.LogInformation("Successfully dispatched all {EventCount} domain events", events.Count);
    }
}
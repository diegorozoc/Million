using Million.PropertiesService.Domain.Common.Events;

namespace Million.PropertiesServices.Application.Common.Events;

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
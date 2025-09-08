namespace Million.PropertiesService.Domain.Common.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
using Million.PropertiesService.Domain.Common.Events;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.Domain.Properties.Events;

public record PropertyCreated : DomainEvent
{
    public Guid PropertyId { get; }
    public string Name { get; }
    public Address Address { get; }
    public Money Price { get; }
    public Guid OwnerId { get; }

    public PropertyCreated(Guid propertyId, string name, Address address, Money price, Guid ownerId)
    {
        PropertyId = propertyId;
        Name = name;
        Address = address;
        Price = price;
        OwnerId = ownerId;
    }
}
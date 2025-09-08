using Million.PropertiesService.Domain.Common.Events;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.Domain.Properties.Events;

public record PropertyPriceChanged : DomainEvent
{
    public Guid PropertyId { get; }
    public Money NewPrice { get; }

    public PropertyPriceChanged(Guid propertyId, Money newPrice)
    {
        PropertyId = propertyId;
        NewPrice = newPrice;
    }
}
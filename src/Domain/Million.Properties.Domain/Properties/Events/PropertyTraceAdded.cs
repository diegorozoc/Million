using Million.PropertiesService.Domain.Common.Events;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.Domain.Properties.Events;

public record PropertyTraceAdded : DomainEvent
{
    public Guid PropertyId { get; }
    public Guid TraceId { get; }
    public Money TraceValue { get; }
    public DateTime SaleDate { get; }
    public decimal TaxPercentage { get; }

    public PropertyTraceAdded(
        Guid propertyId, 
        Guid traceId, 
        Money traceValue, 
        DateTime saleDate, 
        decimal taxPercentage)
    {
        PropertyId = propertyId;
        TraceId = traceId;
        TraceValue = traceValue;
        SaleDate = saleDate;
        TaxPercentage = taxPercentage;
    }
}
using Million.PropertiesService.Domain.Common.Entities;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Properties.Events;

namespace Million.PropertiesService.Domain.Properties.Entities;

public class PropertyTrace : AggregateRoot
{
    public Guid IdPropertyTrace { get; private set; }
    public DateTime DateSale { get; private set; }
    public Money Value { get; private set; }
    public decimal TaxPercentage { get; private set; }
    public Money TaxAmount { get; private set; }
    public Guid IdProperty { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PropertyTrace() { }

    private PropertyTrace(Guid propertyId, Money value, decimal taxPercentage)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));
        if (taxPercentage < 0 || taxPercentage > 100)
            throw new ArgumentException("Tax percentage must be between 0 and 100", nameof(taxPercentage));

        IdPropertyTrace = Guid.NewGuid();
        IdProperty = propertyId;
        DateSale = DateTime.UtcNow;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        TaxPercentage = taxPercentage;
        TaxAmount = CalculateTaxAmount(value, taxPercentage);
        CreatedAt = DateTime.UtcNow;

        // Raise domain event for eventual consistency
        RaiseDomainEvent(new PropertyTraceAdded(propertyId, IdPropertyTrace, value, DateSale, taxPercentage));
    }

    public static PropertyTrace Create(Guid propertyId, Money value, decimal taxPercentage)
    {
        return new PropertyTrace(propertyId, value, taxPercentage);
    }

    private static Money CalculateTaxAmount(Money value, decimal taxPercentage)
    {
        var taxAmount = value.Amount * (taxPercentage / 100);
        return new Money(taxAmount, value.Currency);
    }

    public Money GetNetValue()
    {
        return Value.Subtract(TaxAmount);
    }

    public bool IsRecentSale(int daysThreshold = 30)
    {
        return DateSale >= DateTime.UtcNow.AddDays(-daysThreshold);
    }

    public bool HasSignificantTax(decimal threshold = 5.0m)
    {
        return TaxPercentage >= threshold;
    }
}

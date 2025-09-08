using Million.PropertiesService.Domain.Common.Specifications;
using Million.PropertiesService.Domain.Properties.Entities;

namespace Million.PropertiesService.Domain.Properties.Specifications;

/// <summary>
/// Specification for PropertyTrace by property ID
/// </summary>
public class PropertyTracesByPropertyIdSpecification : BaseSpecification<PropertyTrace>
{
    public PropertyTracesByPropertyIdSpecification(Guid propertyId)
        : base(pt => pt.IdProperty == propertyId)
    {
        ApplyOrderByDescending(pt => pt.DateSale);
    }
}

/// <summary>
/// Specification for PropertyTrace within a value range
/// </summary>
public class PropertyTraceValueRangeSpecification : BaseSpecification<PropertyTrace>
{
    public PropertyTraceValueRangeSpecification(decimal minValue, decimal maxValue)
        : base(pt => pt.Value.Amount >= minValue && pt.Value.Amount <= maxValue)
    {
        ApplyOrderBy(pt => pt.Value.Amount);
    }
}

/// <summary>
/// Specification for PropertyTrace within a date range
/// </summary>
public class PropertyTraceDateRangeSpecification : BaseSpecification<PropertyTrace>
{
    public PropertyTraceDateRangeSpecification(DateTime startDate, DateTime endDate)
        : base(pt => pt.DateSale >= startDate && pt.DateSale <= endDate)
    {
        ApplyOrderByDescending(pt => pt.DateSale);
    }
}

/// <summary>
/// Specification for recent PropertyTrace entries
/// </summary>
public class RecentPropertyTracesSpecification : BaseSpecification<PropertyTrace>
{
    public RecentPropertyTracesSpecification(int days = 30)
        : base(pt => pt.DateSale >= DateTime.UtcNow.AddDays(-days))
    {
        ApplyOrderByDescending(pt => pt.DateSale);
    }
}

/// <summary>
/// Specification for PropertyTrace with high tax rates
/// </summary>
public class HighTaxPropertyTracesSpecification : BaseSpecification<PropertyTrace>
{
    public HighTaxPropertyTracesSpecification(decimal taxThreshold = 5.0m)
        : base(pt => pt.TaxPercentage >= taxThreshold)
    {
        ApplyOrderByDescending(pt => pt.TaxPercentage);
    }
}

/// <summary>
/// Specification for latest PropertyTrace by property ID
/// </summary>
public class LatestPropertyTraceByPropertyIdSpecification : BaseSpecification<PropertyTrace>
{
    public LatestPropertyTraceByPropertyIdSpecification(Guid propertyId)
        : base(pt => pt.IdProperty == propertyId)
    {
        ApplyOrderByDescending(pt => pt.DateSale);
        ApplyPaging(0, 1); // Take only the latest one
    }
}
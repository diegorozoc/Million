using Million.PropertiesService.Domain.Common.Specifications;
using Million.PropertiesService.Domain.Properties.Entities;
using System.Linq.Expressions;

namespace Million.PropertiesService.Domain.Properties.Specifications;

/// <summary>
/// Specification for filtering properties by multiple criteria
/// </summary>
public class PropertyFilterSpecification : BaseSpecification<Property>
{
    public PropertyFilterSpecification(
        string? country = null,
        string? city = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? year = null)
        : base(BuildCriteria(country, city, minPrice, maxPrice, year))
    {
        // Include owner information for complete property data
        AddInclude(p => p.Owner);
    }

    private static Expression<Func<Property, bool>> BuildCriteria(
        string? country,
        string? city,
        decimal? minPrice,
        decimal? maxPrice,
        int? year)
    {
        Expression<Func<Property, bool>> criteria = p => true;

        if (!string.IsNullOrWhiteSpace(country))
        {
            var countryFilter = (Expression<Func<Property, bool>>)(p => 
                p.Address.Country.ToLower().Contains(country.ToLower()));
            criteria = CombineExpressions(criteria, countryFilter);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var cityFilter = (Expression<Func<Property, bool>>)(p => 
                p.Address.City.ToLower().Contains(city.ToLower()));
            criteria = CombineExpressions(criteria, cityFilter);
        }

        if (minPrice.HasValue)
        {
            var minPriceFilter = (Expression<Func<Property, bool>>)(p => 
                p.Price.Amount >= minPrice.Value);
            criteria = CombineExpressions(criteria, minPriceFilter);
        }

        if (maxPrice.HasValue)
        {
            var maxPriceFilter = (Expression<Func<Property, bool>>)(p => 
                p.Price.Amount <= maxPrice.Value);
            criteria = CombineExpressions(criteria, maxPriceFilter);
        }

        if (year.HasValue)
        {
            var yearFilter = (Expression<Func<Property, bool>>)(p => 
                p.Year == year.Value);
            criteria = CombineExpressions(criteria, yearFilter);
        }

        return criteria;
    }

    private static Expression<Func<Property, bool>> CombineExpressions(
        Expression<Func<Property, bool>> left,
        Expression<Func<Property, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(Property));
        var leftInvoke = Expression.Invoke(left, parameter);
        var rightInvoke = Expression.Invoke(right, parameter);
        var and = Expression.AndAlso(leftInvoke, rightInvoke);
        
        return Expression.Lambda<Func<Property, bool>>(and, parameter);
    }
}
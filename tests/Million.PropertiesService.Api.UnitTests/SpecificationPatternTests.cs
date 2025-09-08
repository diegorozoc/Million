using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Specifications;
using Million.PropertiesService.Domain.Owners.Entities;
using NUnit.Framework;
using System.Linq.Expressions;

namespace Million.PropertiesService.Api.UnitTests;

public class SpecificationPatternTests
{
    [Test]
    public void PropertyFilterSpecification_Should_Build_Correct_Criteria()
    {
        // Arrange
        var country = "USA";
        var city = "Miami";
        var minPrice = 100000m;
        var maxPrice = 500000m;
        var year = 2020;

        // Act
        var specification = new PropertyFilterSpecification(country, city, minPrice, maxPrice, year);

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.Includes.Count, Is.EqualTo(1)); // Should include Owner
    }

    [Test]
    public void PropertyFilterSpecification_Should_Handle_Null_Parameters()
    {
        // Arrange & Act
        var specification = new PropertyFilterSpecification();

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.Includes.Count, Is.EqualTo(1)); // Should still include Owner
    }

    [Test]
    public void PropertyTracesByPropertyIdSpecification_Should_Order_By_Date_Descending()
    {
        // Arrange
        var propertyId = Guid.NewGuid();

        // Act
        var specification = new PropertyTracesByPropertyIdSpecification(propertyId);

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.OrderByDescending, Is.Not.Null);
        Assert.That(specification.OrderBy, Is.Null);
    }

    [Test]
    public void PropertyTraceValueRangeSpecification_Should_Order_By_Amount_Ascending()
    {
        // Arrange
        var minValue = 100000m;
        var maxValue = 500000m;

        // Act
        var specification = new PropertyTraceValueRangeSpecification(minValue, maxValue);

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.OrderBy, Is.Not.Null);
        Assert.That(specification.OrderByDescending, Is.Null);
    }

    [Test]
    public void PropertyTraceDateRangeSpecification_Should_Order_By_Date_Descending()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        var specification = new PropertyTraceDateRangeSpecification(startDate, endDate);

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.OrderByDescending, Is.Not.Null);
        Assert.That(specification.OrderBy, Is.Null);
    }

    [Test]
    public void RecentPropertyTracesSpecification_Should_Filter_Recent_Traces()
    {
        // Arrange
        var days = 15;

        // Act
        var specification = new RecentPropertyTracesSpecification(days);

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.OrderByDescending, Is.Not.Null);
    }

    [Test]
    public void HighTaxPropertyTracesSpecification_Should_Filter_High_Tax()
    {
        // Arrange
        var taxThreshold = 10.0m;

        // Act
        var specification = new HighTaxPropertyTracesSpecification(taxThreshold);

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.OrderByDescending, Is.Not.Null);
    }

    [Test]
    public void LatestPropertyTraceByPropertyIdSpecification_Should_Use_Paging()
    {
        // Arrange
        var propertyId = Guid.NewGuid();

        // Act
        var specification = new LatestPropertyTraceByPropertyIdSpecification(propertyId);

        // Assert
        Assert.That(specification.Criteria, Is.Not.Null);
        Assert.That(specification.OrderByDescending, Is.Not.Null);
        Assert.That(specification.IsPagingEnabled, Is.True);
        Assert.That(specification.Take, Is.EqualTo(1));
        Assert.That(specification.Skip, Is.EqualTo(0));
    }

    [Test]
    public void PropertyFilterSpecification_Should_Work_With_Single_Parameter()
    {
        // Arrange & Act
        var countryOnlySpec = new PropertyFilterSpecification(country: "USA");
        var cityOnlySpec = new PropertyFilterSpecification(city: "Miami");
        var priceOnlySpec = new PropertyFilterSpecification(minPrice: 100000m);
        var yearOnlySpec = new PropertyFilterSpecification(year: 2020);

        // Assert
        Assert.That(countryOnlySpec.Criteria, Is.Not.Null);
        Assert.That(cityOnlySpec.Criteria, Is.Not.Null);
        Assert.That(priceOnlySpec.Criteria, Is.Not.Null);
        Assert.That(yearOnlySpec.Criteria, Is.Not.Null);
    }
}
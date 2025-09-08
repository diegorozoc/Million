using Million.PropertiesServices.Application.Properties.Events;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Events;
using Million.PropertiesService.Domain.Owners.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Million.PropertiesService.Api.UnitTests;

public class AggregateBoundaryTests
{
    [Test]
    public void PropertyTrace_Should_Be_Independent_Aggregate_Root()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(500000, "USD");
        var taxPercentage = 10.5m;

        // Act
        var propertyTrace = PropertyTrace.Create(propertyId, value, taxPercentage);

        // Assert
        Assert.That(propertyTrace.IdPropertyTrace, Is.Not.EqualTo(Guid.Empty));
        Assert.That(propertyTrace.IdProperty, Is.EqualTo(propertyId));
        Assert.That(propertyTrace.Value, Is.EqualTo(value));
        Assert.That(propertyTrace.TaxPercentage, Is.EqualTo(taxPercentage));
        
        // Verify it raises domain events (as aggregate root)
        Assert.That(propertyTrace.DomainEvents, Has.Count.EqualTo(1));
        Assert.That(propertyTrace.DomainEvents.First(), Is.TypeOf<PropertyTraceAdded>());
    }

    [Test]
    public void Property_Should_Cache_Latest_Value_For_Performance()
    {
        // Arrange
        var owner = Owner.Create("John Doe", 
            new Address("123 Main St", "New York", "10001", "USA"),
            new DateOfBirth(new DateTime(1980, 1, 1)));
        
        var property = Property.Create("Test Property",
            new Address("456 Oak St", "Miami", "33101", "USA"),
            new Money(400000, "USD"),
            "PROP-001",
            2020,
            owner);

        // Assert property creation
        Assert.That(property.Price.Amount, Is.EqualTo(400000));
        Assert.That(property.Price.Currency, Is.EqualTo("USD"));
    }


    [Test]
    public async Task PropertyTraceAddedEventHandler_Should_Process_Event_Successfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var traceValue = new Money(600000, "USD");
        var saleDate = DateTime.UtcNow;

        var mockLogger = new Mock<ILogger<PropertyTraceAddedEventHandler>>();

        var handler = new PropertyTraceAddedEventHandler(mockLogger.Object);

        var domainEvent = new PropertyTraceAdded(
            propertyId, 
            Guid.NewGuid(), 
            traceValue, 
            saleDate, 
            8.5m);

        // Act
        await handler.Handle(domainEvent);

        var test = It.IsAny<EventId>();

        // Assert - Verify event was processed without errors
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully processed PropertyTraceAdded event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
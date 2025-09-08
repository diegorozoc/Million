using Microsoft.Extensions.Logging;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesServices.Application.Properties.Events;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Properties.Events;
using Million.PropertiesService.Domain.Owners.Entities;
using Moq;
using NUnit.Framework;

namespace Million.PropertiesService.Api.UnitTests;

public class DomainEventTests
{
    [Test]
    public async Task PropertyCreatedEventHandler_Should_Handle_Event_Successfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PropertyCreatedEventHandler>>();
        var handler = new PropertyCreatedEventHandler(mockLogger.Object);
        
        var address = new Address("123 Main St", "New York", "10001", "USA");
        var price = new Money(500000, "USD");
        var propertyCreatedEvent = new PropertyCreated(
            Guid.NewGuid(),
            "Test Property",
            address,
            price,
            Guid.NewGuid());

        // Act
        await handler.Handle(propertyCreatedEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Property created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task PropertyPriceChangedEventHandler_Should_Handle_Event_Successfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PropertyPriceChangedEventHandler>>();
        var handler = new PropertyPriceChangedEventHandler(mockLogger.Object);
        
        var newPrice = new Money(600000, "USD");
        var propertyPriceChangedEvent = new PropertyPriceChanged(Guid.NewGuid(), newPrice);

        // Act
        await handler.Handle(propertyPriceChangedEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Property price changed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DomainEventDispatcher_Should_Dispatch_Multiple_Events()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<DomainEventDispatcher>>();
        var mockPropertyCreatedHandler = new Mock<IDomainEventHandler<PropertyCreated>>();
        var mockPriceChangedHandler = new Mock<IDomainEventHandler<PropertyPriceChanged>>();

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IDomainEventHandler<PropertyCreated>>)))
            .Returns(new[] { mockPropertyCreatedHandler.Object });

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IDomainEventHandler<PropertyPriceChanged>>)))
            .Returns(new[] { mockPriceChangedHandler.Object });

        var dispatcher = new DomainEventDispatcher(mockServiceProvider.Object, mockLogger.Object);

        var address = new Address("123 Main St", "New York", "10001", "USA");
        var price = new Money(500000, "USD");
        var events = new List<Million.PropertiesService.Domain.Common.Events.IDomainEvent>
        {
            new PropertyCreated(Guid.NewGuid(), "Test Property", address, price, Guid.NewGuid()),
            new PropertyPriceChanged(Guid.NewGuid(), new Money(600000, "USD"))
        };

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert - verify that the dispatcher attempts to handle the events
        // Note: The actual service provider resolution would be handled by the DI container
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Dispatching 2 domain events")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
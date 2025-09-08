using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Million.PropertiesService.Application.Properties.Commands.ChangePrice;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain.Common.Events;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Moq;

namespace Million.PropertiesService.Application.UnitTests.Properties.Commands;

[TestFixture]
public class ChangePriceCommandHandlerTests
{
    private Mock<IPropertyRepository> _propertyRepositoryMock;
    private Mock<IValidator<ChangePriceCommand>> _validatorMock;
    private Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
    private ChangePriceCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _validatorMock = new Mock<IValidator<ChangePriceCommand>>();
        _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
        
        _handler = new ChangePriceCommandHandler(
            _propertyRepositoryMock.Object,
            _validatorMock.Object,
            _domainEventDispatcherMock.Object);
    }

    [Test]
    public async Task Handle_ValidRequest_ShouldChangePriceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var oldPrice = new Money(200000, "USD");
        var newPrice = new Money(250000, "USD");
        
        var request = new ChangePriceCommand(newPrice, propertyId);
        
        var ownerAddress = new Address("123 Owner St", "Owner City", "12345", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-30));
        var owner = Owner.Create("Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("456 Property St", "Property City", "54321", "USA");
        var property = Property.Create("Test Property", propertyAddress, oldPrice, "PROP001", 2023, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Price.Should().Be(newPrice);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PriceIncrease_ShouldChangePriceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var oldPrice = new Money(100000, "USD");
        var newPrice = new Money(150000, "USD");
        
        var request = new ChangePriceCommand(newPrice, propertyId);
        
        var ownerAddress = new Address("789 Owner Ave", "Owner City", "67890", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-25));
        var owner = Owner.Create("Price Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("111 Price Test St", "Property City", "11111", "USA");
        var property = Property.Create("Price Test Property", propertyAddress, oldPrice, "PROP002", 2022, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Price.Should().Be(newPrice);
        property.Price.Amount.Should().Be(150000);
        property.Price.Currency.Should().Be("USD");
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PriceDecrease_ShouldChangePriceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var oldPrice = new Money(300000, "USD");
        var newPrice = new Money(275000, "USD");
        
        var request = new ChangePriceCommand(newPrice, propertyId);
        
        var ownerAddress = new Address("222 Decrease Ave", "Owner City", "22222", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-35));
        var owner = Owner.Create("Decrease Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("333 Decrease St", "Property City", "33333", "USA");
        var property = Property.Create("Decrease Test Property", propertyAddress, oldPrice, "PROP003", 2021, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Price.Should().Be(newPrice);
        property.Price.Amount.Should().Be(275000);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public void Handle_InvalidValidation_ShouldThrowValidationException()
    {
        // Arrange  
        var request = new ChangePriceCommand(new Money(1000, "USD"), Guid.Empty);
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Price", "Price must be greater than zero"),
            new ValidationFailure("IdProperty", "Property ID is required")
        });

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Property ID is required"));
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_PropertyNotFound_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new ChangePriceCommand(new Money(200000, "USD"), propertyId);
        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync((Property?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain($"Property with ID {propertyId} not found"));
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ValidZeroPrice_ShouldChangePriceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var oldPrice = new Money(100000, "USD");
        var zeroPrice = new Money(0, "USD"); // Test that zero price is handled correctly
        var request = new ChangePriceCommand(zeroPrice, propertyId);
        
        var ownerAddress = new Address("444 Zero Ave", "Owner City", "44444", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-40));
        var owner = Owner.Create("Zero Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("555 Zero St", "Property City", "55555", "USA");
        var property = Property.Create("Zero Test Property", propertyAddress, oldPrice, "PROP004", 2020, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Price.Should().Be(zeroPrice);
        property.Price.Amount.Should().Be(0);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SamePriceChange_ShouldStillProcessSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var samePrice = new Money(200000, "USD");
        
        var request = new ChangePriceCommand(samePrice, propertyId);
        
        var ownerAddress = new Address("666 Same Ave", "Owner City", "66666", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-28));
        var owner = Owner.Create("Same Price Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("777 Same St", "Property City", "77777", "USA");
        var property = Property.Create("Same Price Property", propertyAddress, samePrice, "PROP005", 2019, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Price.Should().Be(samePrice);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DifferentCurrency_ShouldChangePriceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var oldPrice = new Money(200000, "USD");
        var newPrice = new Money(250000, "EUR");
        
        var request = new ChangePriceCommand(newPrice, propertyId);
        
        var ownerAddress = new Address("888 Euro Ave", "Owner City", "88888", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-32));
        var owner = Owner.Create("Euro Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("999 Euro St", "Property City", "99999", "USA");
        var property = Property.Create("Euro Test Property", propertyAddress, oldPrice, "PROP006", 2018, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Price.Should().Be(newPrice);
        property.Price.Currency.Should().Be("EUR");
        property.Price.Amount.Should().Be(250000);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public async Task Handle_MultipleConsecutivePriceChanges_ShouldProcessEachSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var initialPrice = new Money(100000, "USD");
        var firstNewPrice = new Money(120000, "USD");
        var secondNewPrice = new Money(130000, "USD");
        var thirdNewPrice = new Money(125000, "USD");
        
        var ownerAddress = new Address("111 Multiple Ave", "Owner City", "11111", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-33));
        var owner = Owner.Create("Multiple Changes Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("222 Multiple St", "Property City", "22222", "USA");
        var property = Property.Create("Multiple Changes Property", propertyAddress, initialPrice, "PROP007", 2017, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ChangePriceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        var firstRequest = new ChangePriceCommand(firstNewPrice, propertyId);
        var secondRequest = new ChangePriceCommand(secondNewPrice, propertyId);
        var thirdRequest = new ChangePriceCommand(thirdNewPrice, propertyId);

        // Act
        await _handler.Handle(firstRequest, CancellationToken.None);
        await _handler.Handle(secondRequest, CancellationToken.None);
        await _handler.Handle(thirdRequest, CancellationToken.None);

        // Assert
        property.Price.Should().Be(thirdNewPrice);
        property.Price.Amount.Should().Be(125000);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Exactly(3));
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Exactly(3));
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}
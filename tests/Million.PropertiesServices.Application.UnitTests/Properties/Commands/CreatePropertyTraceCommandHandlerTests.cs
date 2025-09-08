using FluentValidation;
using FluentValidation.Results;
using FluentAssertions;
using Moq;
using Million.PropertiesServices.Application.Properties.Commands.CreatePropertyTrace;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Common.Events;

namespace Million.PropertiesService.Application.UnitTests.Properties.Commands;

[TestFixture]
public class CreatePropertyTraceCommandHandlerTests
{
    private Mock<IPropertyTraceRepository> _propertyTraceRepositoryMock;
    private Mock<IPropertyRepository> _propertyRepositoryMock;
    private Mock<IValidator<CreatePropertyTraceCommand>> _validatorMock;
    private Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
    private CreatePropertyTraceCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _propertyTraceRepositoryMock = new Mock<IPropertyTraceRepository>();
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _validatorMock = new Mock<IValidator<CreatePropertyTraceCommand>>();
        _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();

        _handler = new CreatePropertyTraceCommandHandler(
            _propertyTraceRepositoryMock.Object,
            _propertyRepositoryMock.Object,
            _validatorMock.Object,
            _domainEventDispatcherMock.Object);
    }

    [Test]
    public async Task Handle_ValidRequest_ShouldCreatePropertyTraceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = 10.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .Returns(Task.CompletedTask);

        _domainEventDispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdPropertyTrace.Should().NotBeEmpty();

        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Once);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestWithDifferentCurrency_ShouldCreatePropertyTraceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(75000, "EUR");
        var taxPercentage = 15.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .Returns(Task.CompletedTask);

        _domainEventDispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdPropertyTrace.Should().NotBeEmpty();
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Once);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestWithZeroTax_ShouldCreatePropertyTraceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(50000, "USD");
        var taxPercentage = 0.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .Returns(Task.CompletedTask);

        _domainEventDispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdPropertyTrace.Should().NotBeEmpty();
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Once);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestWithMaxTax_ShouldCreatePropertyTraceSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(200000, "USD");
        var taxPercentage = 100.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .Returns(Task.CompletedTask);

        _domainEventDispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdPropertyTrace.Should().NotBeEmpty();
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Once);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_ValidationFails_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = 10.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        var validationFailures = new List<ValidationFailure>
        {
            new("PropertyId", "Property ID is required."),
            new("Value", "Value is required.")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Never);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_PropertyNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = 10.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, CancellationToken.None));
        exception.Message.Should().Contain($"Property with ID '{propertyId}' does not exist.");

        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Never);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_EmptyPropertyId_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.Empty;
        var value = new Money(100000, "USD");
        var taxPercentage = 10.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Never);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_NegativeTaxPercentage_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = -5.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Never);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_TaxPercentageOver100_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = 150.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Never);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_MultiplePropertyTraces_ShouldCreateEachSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value1 = new Money(100000, "USD");
        var value2 = new Money(150000, "USD");
        var taxPercentage = 10.0m;
        
        var request1 = new CreatePropertyTraceCommand(propertyId, value1, taxPercentage);
        var request2 = new CreatePropertyTraceCommand(propertyId, value2, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreatePropertyTraceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(request1, CancellationToken.None);
        var result2 = await _handler.Handle(request2, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result1.IdPropertyTrace.Should().NotBeEmpty();
        result2.Should().NotBeNull();
        result2.IdPropertyTrace.Should().NotBeEmpty();
        result1.IdPropertyTrace.Should().NotBe(result2.IdPropertyTrace);

        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Exactly(2));
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public void Handle_NullValue_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var taxPercentage = 10.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, null!, taxPercentage);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Never);
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ValidRequest_ShouldCreatePropertyTraceWithCorrectTaxCalculation()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = 15.0m;
        var expectedTaxAmount = new Money(15000, "USD");
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);
        PropertyTrace? savedPropertyTrace = null;

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .Callback<PropertyTrace>(pt => savedPropertyTrace = pt)
            .Returns(Task.CompletedTask);

        _domainEventDispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdPropertyTrace.Should().NotBeEmpty();
        
        savedPropertyTrace.Should().NotBeNull();
        savedPropertyTrace!.IdProperty.Should().Be(propertyId);
        savedPropertyTrace.Value.Should().Be(value);
        savedPropertyTrace.TaxPercentage.Should().Be(taxPercentage);
        savedPropertyTrace.TaxAmount.Amount.Should().Be(expectedTaxAmount.Amount);
        savedPropertyTrace.TaxAmount.Currency.Should().Be(expectedTaxAmount.Currency);
        savedPropertyTrace.DateSale.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedPropertyTrace.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = 10.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);
        var expectedException = new InvalidOperationException("Repository error");

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, CancellationToken.None));
        
        _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_DomainEventDispatcherThrowsException_ShouldPropagateException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var value = new Money(100000, "USD");
        var taxPercentage = 10.0m;
        
        var request = new CreatePropertyTraceCommand(propertyId, value, taxPercentage);
        var expectedException = new InvalidOperationException("Event dispatcher error");

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _propertyRepositoryMock
            .Setup(r => r.ExistsAsync(propertyId))
            .ReturnsAsync(true);

        _propertyTraceRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PropertyTrace>()))
            .Returns(Task.CompletedTask);

        _domainEventDispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, CancellationToken.None));
        
        _propertyTraceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<PropertyTrace>()), Times.Once);
    }
}
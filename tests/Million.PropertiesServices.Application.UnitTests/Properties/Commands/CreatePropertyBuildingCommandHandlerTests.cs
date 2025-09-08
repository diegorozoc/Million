using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesServices.Application.Properties.Commands.CreatePropertyBuilding;
using Million.PropertiesService.Domain.Common.Events;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Owners.Repositories;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Properties.Services;
using Moq;

namespace Million.PropertiesService.Application.UnitTests.Properties.Commands;

[TestFixture]
public class CreatePropertyBuildingCommandHandlerTests
{
    private Mock<IPropertyRepository> _propertyRepositoryMock;
    private Mock<IOwnerRepository> _ownerRepositoryMock;
    private Mock<IValidator<CreatePropertyBuildingCommand>> _validatorMock;
    private Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
    private Mock<IPropertyValidationService> _propertyValidationServiceMock;
    private Mock<IPropertyOwnershipService> _propertyOwnershipServiceMock;
    private CreatePropertyBuildingCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _ownerRepositoryMock = new Mock<IOwnerRepository>();
        _validatorMock = new Mock<IValidator<CreatePropertyBuildingCommand>>();
        _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _propertyValidationServiceMock = new Mock<IPropertyValidationService>();
        _propertyOwnershipServiceMock = new Mock<IPropertyOwnershipService>();
        
        _handler = new CreatePropertyBuildingCommandHandler(
            _propertyRepositoryMock.Object,
            _ownerRepositoryMock.Object,
            _validatorMock.Object,
            _domainEventDispatcherMock.Object,
            _propertyValidationServiceMock.Object,
            _propertyOwnershipServiceMock.Object);
    }

    [Test]
    public async Task Handle_ValidRequest_ShouldCreatePropertyAndReturnResponse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var propertyAddress = new Address("123 Property St", "Property City", "12345", "USA");
        var price = new Money(300000, "USD");
        var request = new CreatePropertyBuildingCommand(
            "Beautiful Villa",
            propertyAddress,
            price,
            "VILLA001",
            2023,
            ownerId);

        var ownerAddress = new Address("456 Owner Ave", "Owner City", "67890", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-35));
        var owner = Owner.Create("Property Owner", ownerAddress, ownerBirthDate);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();
        var ownershipValidation = PropertyOwnershipValidationResult.Success();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);
        
        _propertyOwnershipServiceMock.Setup(x => x.ValidateOwnerCanAcquirePropertyAsync(owner, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownershipValidation);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdProperty.Should().NotBeEmpty();
        
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Once);
        _propertyOwnershipServiceMock.Verify(x => x.AssignPropertyToOwnerAsync(It.IsAny<Property>(), owner, It.IsAny<CancellationToken>()), Times.Once);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestDifferentCurrency_ShouldCreatePropertySuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var propertyAddress = new Address("789 Euro St", "Euro City", "54321", "Germany");
        var price = new Money(250000, "EUR");
        var request = new CreatePropertyBuildingCommand(
            "European Apartment",
            propertyAddress,
            price,
            "EURO001",
            2022,
            ownerId);

        var ownerAddress = new Address("111 European Ave", "Owner City", "11111", "Germany");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-42));
        var owner = Owner.Create("European Owner", ownerAddress, ownerBirthDate);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();
        var ownershipValidation = PropertyOwnershipValidationResult.Success();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);
        
        _propertyOwnershipServiceMock.Setup(x => x.ValidateOwnerCanAcquirePropertyAsync(owner, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownershipValidation);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdProperty.Should().NotBeEmpty();
        
        _propertyValidationServiceMock.Verify(x => x.ValidatePropertyForCreationAsync(
            "European Apartment", "EURO001", 2022, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_InvalidValidation_ShouldThrowValidationException()
    {
        // Arrange
        var propertyAddress = new Address("Invalid St", "Invalid City", "00000", "USA");
        var price = new Money(100000, "USD");
        var request = new CreatePropertyBuildingCommand(
            "",
            propertyAddress,
            price,
            "",
            0,
            Guid.Empty);

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Property name is required"),
            new ValidationFailure("CodeInternal", "Property code is required"),
            new ValidationFailure("Year", "Year must be greater than 1800"),
            new ValidationFailure("IdOwner", "Owner ID is required")
        });

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Property name is required"));
        Assert.That(exception.Message, Does.Contain("Property code is required"));
        
        _propertyValidationServiceMock.Verify(x => x.ValidatePropertyForCreationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public void Handle_PropertyValidationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var propertyAddress = new Address("Duplicate St", "Duplicate City", "55555", "USA");
        var price = new Money(200000, "USD");
        var request = new CreatePropertyBuildingCommand(
            "Duplicate Property",
            propertyAddress,
            price,
            "DUPLICATE001",
            2023,
            ownerId);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Failure("Property code 'DUPLICATE001' already exists");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Property code 'DUPLICATE001' already exists"));
        
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public void Handle_OwnerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var propertyAddress = new Address("Orphan St", "Orphan City", "66666", "USA");
        var price = new Money(180000, "USD");
        var request = new CreatePropertyBuildingCommand(
            "Orphan Property",
            propertyAddress,
            price,
            "ORPHAN001",
            2023,
            ownerId);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync((Owner?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain($"Owner with ID '{ownerId}' does not exist"));
        
        _propertyOwnershipServiceMock.Verify(x => x.ValidateOwnerCanAcquirePropertyAsync(
            It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public void Handle_OwnershipValidationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var propertyAddress = new Address("Limit St", "Limit City", "77777", "USA");
        var price = new Money(350000, "USD");
        var request = new CreatePropertyBuildingCommand(
            "Over Limit Property",
            propertyAddress,
            price,
            "LIMIT001",
            2023,
            ownerId);

        var ownerAddress = new Address("MaxOut Ave", "MaxOut City", "88888", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-50));
        var owner = Owner.Create("Maxed Out Owner", ownerAddress, ownerBirthDate);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();
        var ownershipValidation = PropertyOwnershipValidationResult.Failure("Owner has reached the maximum number of properties they can own");

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);
        
        _propertyOwnershipServiceMock.Setup(x => x.ValidateOwnerCanAcquirePropertyAsync(owner, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownershipValidation);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Owner has reached the maximum number of properties"));
        
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public async Task Handle_ValidRequestWithZeroPrice_ShouldCreatePropertySuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var propertyAddress = new Address("Zero Price St", "Zero Price City", "99999", "USA");
        var price = new Money(0, "USD"); // Test that zero price is handled correctly by domain
        var request = new CreatePropertyBuildingCommand(
            "Zero Price Property",
            propertyAddress,
            price,
            "ZERO001",
            2023,
            ownerId);

        var ownerAddress = new Address("Valid Owner Ave", "Valid City", "12321", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-30));
        var owner = Owner.Create("Valid Owner", ownerAddress, ownerBirthDate);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();
        var ownershipValidation = PropertyOwnershipValidationResult.Success();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);
        
        _propertyOwnershipServiceMock.Setup(x => x.ValidateOwnerCanAcquirePropertyAsync(owner, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownershipValidation);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdProperty.Should().NotBeEmpty();
        
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Once);
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestWithOldYear_ShouldCreatePropertySuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var propertyAddress = new Address("Historic St", "Historic City", "18001", "USA");
        var price = new Money(500000, "USD");
        var request = new CreatePropertyBuildingCommand(
            "Historic Property",
            propertyAddress,
            price,
            "HISTORIC001",
            1900, // Old year but should be valid
            ownerId);

        var ownerAddress = new Address("Historic Owner Ave", "Historic City", "18002", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-60));
        var owner = Owner.Create("Historic Owner", ownerAddress, ownerBirthDate);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();
        var ownershipValidation = PropertyOwnershipValidationResult.Success();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);
        
        _propertyOwnershipServiceMock.Setup(x => x.ValidateOwnerCanAcquirePropertyAsync(owner, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownershipValidation);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdProperty.Should().NotBeEmpty();
        
        _propertyValidationServiceMock.Verify(x => x.ValidatePropertyForCreationAsync(
            "Historic Property", "HISTORIC001", 1900, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestCurrentYear_ShouldCreatePropertySuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var currentYear = DateTime.Now.Year;
        var propertyAddress = new Address("New St", "New City", $"{currentYear}", "USA");
        var price = new Money(400000, "USD");
        var request = new CreatePropertyBuildingCommand(
            "Brand New Property",
            propertyAddress,
            price,
            $"NEW{currentYear}",
            currentYear,
            ownerId);

        var ownerAddress = new Address("New Owner Ave", "New City", $"{currentYear + 1}", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-25));
        var owner = Owner.Create("New Owner", ownerAddress, ownerBirthDate);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();
        var ownershipValidation = PropertyOwnershipValidationResult.Success();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);
        
        _propertyOwnershipServiceMock.Setup(x => x.ValidateOwnerCanAcquirePropertyAsync(owner, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownershipValidation);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdProperty.Should().NotBeEmpty();
        
        _propertyValidationServiceMock.Verify(x => x.ValidatePropertyForCreationAsync(
            "Brand New Property", $"NEW{currentYear}", currentYear, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestMultipleDifferentProperties_ShouldCreateEachSuccessfully()
    {
        // Arrange
        var firstOwnerId = Guid.NewGuid();
        var secondOwnerId = Guid.NewGuid();

        var firstPropertyAddress = new Address("First St", "First City", "11111", "USA");
        var secondPropertyAddress = new Address("Second St", "Second City", "22222", "Canada");

        var firstRequest = new CreatePropertyBuildingCommand(
            "First Property",
            firstPropertyAddress,
            new Money(250000, "USD"),
            "FIRST001",
            2023,
            firstOwnerId);

        var secondRequest = new CreatePropertyBuildingCommand(
            "Second Property",
            secondPropertyAddress,
            new Money(300000, "CAD"),
            "SECOND001",
            2022,
            secondOwnerId);

        var firstOwnerAddress = new Address("First Owner Ave", "First City", "11112", "USA");
        var firstOwnerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-30));
        var firstOwner = Owner.Create("First Owner", firstOwnerAddress, firstOwnerBirthDate);

        var secondOwnerAddress = new Address("Second Owner Ave", "Second City", "22223", "Canada");
        var secondOwnerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-35));
        var secondOwner = Owner.Create("Second Owner", secondOwnerAddress, secondOwnerBirthDate);

        var validationResult = new ValidationResult();
        var propertyValidation = PropertyValidationResult.Success();
        var ownershipValidation = PropertyOwnershipValidationResult.Success();

        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreatePropertyBuildingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyValidationServiceMock.Setup(x => x.ValidatePropertyForCreationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyValidation);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(firstOwnerId))
            .ReturnsAsync(firstOwner);
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(secondOwnerId))
            .ReturnsAsync(secondOwner);
        
        _propertyOwnershipServiceMock.Setup(x => x.ValidateOwnerCanAcquirePropertyAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownershipValidation);

        // Act
        var firstResult = await _handler.Handle(firstRequest, CancellationToken.None);
        var secondResult = await _handler.Handle(secondRequest, CancellationToken.None);

        // Assert
        firstResult.Should().NotBeNull();
        firstResult.IdProperty.Should().NotBeEmpty();
        
        secondResult.Should().NotBeNull();
        secondResult.IdProperty.Should().NotBeEmpty();
        
        firstResult.IdProperty.Should().NotBe(secondResult.IdProperty);
        
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Exactly(2));
        _domainEventDispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Million.PropertiesService.Application.Properties.Commands.UpdateProperty;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Owners.Repositories;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Moq;

namespace Million.PropertiesService.Application.UnitTests.Properties.Commands;

[TestFixture]
public class UpdatePropertyCommandHandlerTests
{
    private Mock<IPropertyRepository> _propertyRepositoryMock;
    private Mock<IOwnerRepository> _ownerRepositoryMock;
    private Mock<IValidator<UpdatePropertyCommand>> _validatorMock;
    private UpdatePropertyCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _ownerRepositoryMock = new Mock<IOwnerRepository>();
        _validatorMock = new Mock<IValidator<UpdatePropertyCommand>>();
        
        _handler = new UpdatePropertyCommandHandler(
            _propertyRepositoryMock.Object,
            _ownerRepositoryMock.Object,
            _validatorMock.Object);
    }

    [Test]
    public async Task Handle_UpdateAllFields_ShouldUpdatePropertySuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var newAddress = new Address("456 Updated St", "Updated City", "54321", "Updated Country");
        
        var request = new UpdatePropertyCommand(
            "Updated Property Name",
            newAddress,
            2024,
            newOwnerId,
            propertyId);

        // Original property setup
        var originalOwnerAddress = new Address("123 Owner St", "Owner City", "12345", "USA");
        var originalOwnerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-30));
        var originalOwner = Owner.Create("Original Owner", originalOwnerAddress, originalOwnerBirthDate);
        var originalPropertyAddress = new Address("123 Original St", "Original City", "12345", "Original Country");
        var property = Property.Create("Original Property", originalPropertyAddress, new Money(200000, "USD"), "PROP001", 2020, originalOwner);

        // New owner setup
        var newOwnerAddress = new Address("789 New Owner St", "New Owner City", "67890", "USA");
        var newOwnerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-35));
        var newOwner = Owner.Create("New Owner", newOwnerAddress, newOwnerBirthDate);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(newOwnerId))
            .ReturnsAsync(newOwner);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Name.Should().Be("Updated Property Name");
        property.Address.Should().Be(newAddress);
        property.Year.Should().Be(2024);
        property.Owner.Should().Be(newOwner);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(newOwnerId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public async Task Handle_UpdateNameOnly_ShouldUpdateNameOnlyAndLeaveOtherFieldsUnchanged()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new UpdatePropertyCommand(
            "Updated Name Only",
            null,
            null,
            null,
            propertyId);

        var ownerAddress = new Address("123 Owner St", "Owner City", "12345", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-40));
        var owner = Owner.Create("Original Owner", ownerAddress, ownerBirthDate);
        var originalAddress = new Address("123 Original St", "Original City", "12345", "Original Country");
        var property = Property.Create("Original Name", originalAddress, new Money(300000, "USD"), "PROP002", 2019, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Name.Should().Be("Updated Name Only");
        property.Address.Should().Be(originalAddress); // Should remain unchanged
        property.Year.Should().Be(2019); // Should remain unchanged
        property.Owner.Should().Be(owner); // Should remain unchanged
        
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public async Task Handle_UpdateAddressOnly_ShouldUpdateAddressOnlyAndLeaveOtherFieldsUnchanged()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newAddress = new Address("999 New Address St", "New Address City", "99999", "New Address Country");
        var request = new UpdatePropertyCommand(
            null,
            newAddress,
            null,
            null,
            propertyId);

        var ownerAddress = new Address("123 Owner St", "Owner City", "12345", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-25));
        var owner = Owner.Create("Address Test Owner", ownerAddress, ownerBirthDate);
        var originalAddress = new Address("111 Old Address St", "Old Address City", "11111", "Old Country");
        var property = Property.Create("Address Test Property", originalAddress, new Money(250000, "EUR"), "PROP003", 2021, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Name.Should().Be("Address Test Property"); // Should remain unchanged
        property.Address.Should().Be(newAddress); // Should be updated
        property.Year.Should().Be(2021); // Should remain unchanged
        property.Owner.Should().Be(owner); // Should remain unchanged
        
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public async Task Handle_UpdateYearOnly_ShouldUpdateYearOnlyAndLeaveOtherFieldsUnchanged()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new UpdatePropertyCommand(
            null,
            null,
            2025,
            null,
            propertyId);

        var ownerAddress = new Address("222 Year Owner St", "Year City", "22222", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-50));
        var owner = Owner.Create("Year Test Owner", ownerAddress, ownerBirthDate);
        var originalAddress = new Address("333 Year Property St", "Year Property City", "33333", "Year Country");
        var property = Property.Create("Year Test Property", originalAddress, new Money(400000, "USD"), "PROP004", 2018, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Name.Should().Be("Year Test Property"); // Should remain unchanged
        property.Address.Should().Be(originalAddress); // Should remain unchanged
        property.Year.Should().Be(2025); // Should be updated
        property.Owner.Should().Be(owner); // Should remain unchanged
        
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public async Task Handle_UpdateOwnerOnly_ShouldUpdateOwnerOnlyAndLeaveOtherFieldsUnchanged()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var request = new UpdatePropertyCommand(
            null,
            null,
            null,
            newOwnerId,
            propertyId);

        // Original owner and property
        var originalOwnerAddress = new Address("444 Original Owner St", "Original Owner City", "44444", "USA");
        var originalOwnerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-45));
        var originalOwner = Owner.Create("Original Owner", originalOwnerAddress, originalOwnerBirthDate);
        var propertyAddress = new Address("555 Owner Test St", "Owner Test City", "55555", "Owner Test Country");
        var property = Property.Create("Owner Test Property", propertyAddress, new Money(350000, "CAD"), "PROP005", 2022, originalOwner);

        // New owner
        var newOwnerAddress = new Address("666 New Owner St", "New Owner City", "66666", "Canada");
        var newOwnerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-38));
        var newOwner = Owner.Create("New Owner", newOwnerAddress, newOwnerBirthDate);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(newOwnerId))
            .ReturnsAsync(newOwner);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Name.Should().Be("Owner Test Property"); // Should remain unchanged
        property.Address.Should().Be(propertyAddress); // Should remain unchanged
        property.Year.Should().Be(2022); // Should remain unchanged
        property.Owner.Should().Be(newOwner); // Should be updated
        
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(newOwnerId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public async Task Handle_NoFieldsToUpdate_ShouldStillSaveProperty()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new UpdatePropertyCommand(
            null,
            null,
            null,
            null,
            propertyId);

        var ownerAddress = new Address("777 No Change Owner St", "No Change City", "77777", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-32));
        var owner = Owner.Create("No Change Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("888 No Change St", "No Change City", "88888", "No Change Country");
        var property = Property.Create("No Change Property", propertyAddress, new Money(275000, "USD"), "PROP006", 2020, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        // All fields should remain unchanged
        property.Name.Should().Be("No Change Property");
        property.Address.Should().Be(propertyAddress);
        property.Year.Should().Be(2020);
        property.Owner.Should().Be(owner);
        
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public void Handle_InvalidValidation_ShouldThrowValidationException()
    {
        // Arrange
        var request = new UpdatePropertyCommand(
            "",
            null,
            0,
            Guid.Empty,
            Guid.Empty);

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("IdProperty", "Property ID is required"),
            new ValidationFailure("Name", "Name cannot be empty when provided"),
            new ValidationFailure("Year", "Year must be greater than 1800 when provided")
        });

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Property ID is required"));
        Assert.That(exception.Message, Does.Contain("Name cannot be empty"));
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public void Handle_PropertyNotFound_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new UpdatePropertyCommand(
            "Property Not Found Update",
            null,
            null,
            null,
            propertyId);

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
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public void Handle_NewOwnerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var nonExistentOwnerId = Guid.NewGuid();
        var request = new UpdatePropertyCommand(
            null,
            null,
            null,
            nonExistentOwnerId,
            propertyId);

        var ownerAddress = new Address("999 Existing Owner St", "Existing City", "99999", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-28));
        var existingOwner = Owner.Create("Existing Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("123 Test St", "Test City", "12345", "Test Country");
        var property = Property.Create("Test Property", propertyAddress, new Money(180000, "USD"), "PROP007", 2023, existingOwner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);
        
        _ownerRepositoryMock.Setup(x => x.GetByIdAsync(nonExistentOwnerId))
            .ReturnsAsync((Owner?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain($"Owner with ID '{nonExistentOwnerId}' does not exist"));
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(nonExistentOwnerId), Times.Once);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public async Task Handle_UpdateMultipleFieldsPartially_ShouldUpdateOnlySpecifiedFields()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newAddress = new Address("111 Partial Update St", "Partial City", "11111", "Partial Country");
        var request = new UpdatePropertyCommand(
            "Partially Updated Property",
            newAddress,
            null, // Year not updated
            null, // Owner not updated
            propertyId);

        var ownerAddress = new Address("222 Partial Owner St", "Partial Owner City", "22222", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-42));
        var owner = Owner.Create("Partial Owner", ownerAddress, ownerBirthDate);
        var originalAddress = new Address("333 Original St", "Original City", "33333", "Original Country");
        var property = Property.Create("Original Property Name", originalAddress, new Money(320000, "EUR"), "PROP008", 2017, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        property.Name.Should().Be("Partially Updated Property"); // Updated
        property.Address.Should().Be(newAddress); // Updated
        property.Year.Should().Be(2017); // Should remain unchanged
        property.Owner.Should().Be(owner); // Should remain unchanged
        
        _ownerRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyRepositoryMock.Verify(x => x.SaveAsync(property), Times.Once);
    }

    [Test]
    public void Handle_DomainValidationError_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new UpdatePropertyCommand(
            "", // Empty string might cause domain validation error
            null,
            null,
            null,
            propertyId);

        var ownerAddress = new Address("444 Domain Error Owner St", "Domain Error City", "44444", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-33));
        var owner = Owner.Create("Domain Error Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("555 Domain Error St", "Domain Error City", "55555", "Domain Error Country");
        var property = Property.Create("Domain Error Property", propertyAddress, new Money(220000, "USD"), "PROP009", 2019, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Domain validation failed"));
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
    }
}
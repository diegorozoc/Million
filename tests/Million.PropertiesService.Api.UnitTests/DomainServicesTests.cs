using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Properties.Services;
using Million.PropertiesService.Domain.Owners.Entities;
using Moq;
using NUnit.Framework;

namespace Million.PropertiesService.Api.UnitTests;

public class DomainServicesTests
{
    [Test]
    public async Task PropertyValidationService_Should_Reject_Duplicate_Code_Internal()
    {
        // Arrange
        var mockPropertyRepository = new Mock<IPropertyRepository>();
        mockPropertyRepository
            .Setup(r => r.CodeInternalExistsAsync("EXISTING-CODE"))
            .ReturnsAsync(true);

        var service = new PropertyValidationService(mockPropertyRepository.Object);

        // Act
        var result = await service.ValidateCodeInternalUniquenessAsync("EXISTING-CODE");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("already exists"));
    }

    [Test]
    public async Task PropertyValidationService_Should_Accept_Unique_Code_Internal()
    {
        // Arrange
        var mockPropertyRepository = new Mock<IPropertyRepository>();
        mockPropertyRepository
            .Setup(r => r.CodeInternalExistsAsync("UNIQUE-CODE"))
            .ReturnsAsync(false);

        var service = new PropertyValidationService(mockPropertyRepository.Object);

        // Act
        var result = await service.ValidateCodeInternalUniquenessAsync("UNIQUE-CODE");

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.ErrorMessage, Is.Empty);
    }

    [Test]
    public async Task PropertyValidationService_Should_Validate_Property_Creation_Rules()
    {
        // Arrange
        var mockPropertyRepository = new Mock<IPropertyRepository>();
        mockPropertyRepository
            .Setup(r => r.CodeInternalExistsAsync("VALID-CODE"))
            .ReturnsAsync(false);

        var service = new PropertyValidationService(mockPropertyRepository.Object);

        // Act
        var result = await service.ValidatePropertyForCreationAsync("Valid Name", "VALID-CODE", 2023);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task PropertyValidationService_Should_Reject_Invalid_Year()
    {
        // Arrange
        var mockPropertyRepository = new Mock<IPropertyRepository>();
        var service = new PropertyValidationService(mockPropertyRepository.Object);

        // Act
        var result = await service.ValidatePropertyForCreationAsync("Valid Name", "VALID-CODE", 1799);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("year must be between"));
    }

    [Test]
    public async Task PropertyOwnershipService_Should_Reject_Minor_Owner()
    {
        // Arrange
        var mockPropertyRepository = new Mock<IPropertyRepository>();
        var service = new PropertyOwnershipService(mockPropertyRepository.Object);

        // Create minor owner (under 18)
        var minorOwner = Owner.Create("Minor Owner",
            new Address("123 Main St", "City", "12345", "Country"),
            new DateOfBirth(DateTime.Now.AddYears(-10))); // 10 years old

        // Act
        var result = await service.ValidateOwnerCanAcquirePropertyAsync(minorOwner);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("must be at least 18 years old"));
    }

    [Test]
    public async Task PropertyOwnershipService_Should_Accept_Adult_Owner()
    {
        // Arrange
        var mockPropertyRepository = new Mock<IPropertyRepository>();
        var service = new PropertyOwnershipService(mockPropertyRepository.Object);

        // Create adult owner
        var adultOwner = Owner.Create("Adult Owner",
            new Address("123 Main St", "City", "12345", "Country"),
            new DateOfBirth(DateTime.Now.AddYears(-25))); // 25 years old

        // Act
        var result = await service.ValidateOwnerCanAcquirePropertyAsync(adultOwner);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.ErrorMessage, Is.Empty);
    }

    [Test]
    public async Task PropertyOwnershipService_Should_Assign_Property_To_Valid_Owner()
    {
        // Arrange
        var mockPropertyRepository = new Mock<IPropertyRepository>();
        var service = new PropertyOwnershipService(mockPropertyRepository.Object);

        var adultOwner = Owner.Create("Valid Owner",
            new Address("123 Main St", "City", "12345", "Country"),
            new DateOfBirth(DateTime.Now.AddYears(-30)));

        var property = Property.Create("Test Property",
            new Address("456 Oak St", "City", "54321", "Country"),
            new Money(500000, "USD"),
            "PROP-001",
            2020,
            adultOwner);

        // Act & Assert (should not throw)
        await service.AssignPropertyToOwnerAsync(property, adultOwner);
        
        // Verify property is assigned to owner
        Assert.That(property.IdOwner, Is.EqualTo(adultOwner.IdOwner));
        Assert.That(adultOwner.PropertyIds, Contains.Item(property.IdProperty));
    }
}
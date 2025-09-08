using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Persistance.Owners.Repositories;

namespace Million.PropertiesService.Persistance.UnitTests.Repositories;

[TestFixture]
public class OwnerRepositoryTests
{
    private PropertiesDbContext _context = null!;
    private OwnerRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PropertiesDbContext(options);
        _repository = new OwnerRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_ExistingOwner_ShouldReturnOwner()
    {
        // Arrange
        var owner = CreateTestOwner("John Doe");
        
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(owner.IdOwner);

        // Assert
        result.Should().NotBeNull();
        result!.IdOwner.Should().Be(owner.IdOwner);
        result.Name.Should().Be("John Doe");
    }

    [Test]
    public async Task GetByIdAsync_NonExistingOwner_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetByNameAsync_ExistingName_ShouldReturnOwner()
    {
        // Arrange
        var owner = CreateTestOwner("Jane Smith");
        
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Jane Smith");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Jane Smith");
    }

    [Test]
    public async Task GetByNameAsync_NonExistingName_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("Non Existing Name");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task SaveAsync_NewOwner_ShouldAddOwner()
    {
        // Arrange
        var owner = CreateTestOwner("New Owner");

        // Act
        await _repository.SaveAsync(owner);

        // Assert
        var savedOwner = await _context.Owners.FindAsync(owner.IdOwner);
        savedOwner.Should().NotBeNull();
        savedOwner!.Name.Should().Be("New Owner");
    }

    [Test]
    public async Task SaveAsync_ExistingOwner_ShouldUpdateOwner()
    {
        // Arrange
        var owner = CreateTestOwner("Original Name");
        
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        owner.UpdateName("Updated Name");

        // Act
        await _repository.SaveAsync(owner);

        // Assert
        var updatedOwner = await _context.Owners.FindAsync(owner.IdOwner);
        updatedOwner.Should().NotBeNull();
        updatedOwner!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task DeleteAsync_ExistingOwner_ShouldRemoveOwner()
    {
        // Arrange
        var owner = CreateTestOwner("Owner to Delete");
        
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(owner.IdOwner);

        // Assert
        var deletedOwner = await _context.Owners.FindAsync(owner.IdOwner);
        deletedOwner.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_NonExistingOwner_ShouldNotThrow()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await _repository.Invoking(r => r.DeleteAsync(nonExistingId))
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task ExistsAsync_ExistingOwner_ShouldReturnTrue()
    {
        // Arrange
        var owner = CreateTestOwner("Existing Owner");
        
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(owner.IdOwner);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_NonExistingOwner_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistingId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetOwnersWithPropertiesAsync_OwnersWithProperties_ShouldReturnOwnersWithProperties()
    {
        // Arrange
        var ownerWithProperties = CreateTestOwner("Owner With Properties");
        var ownerWithoutProperties = CreateTestOwner("Owner Without Properties");
        
        // Add a property to the first owner
        var property = CreateTestProperty("Test Property", ownerWithProperties);
        ownerWithProperties.AddProperty(property.IdProperty);
        
        await _context.Owners.AddRangeAsync(ownerWithProperties, ownerWithoutProperties);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOwnersWithPropertiesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(o => o.IdOwner == ownerWithProperties.IdOwner);
        result.Should().NotContain(o => o.IdOwner == ownerWithoutProperties.IdOwner);
    }

    [Test]
    public async Task GetAdultOwnersAsync_MixedAges_ShouldReturnOnlyAdults()
    {
        // Arrange
        var adultOwner = CreateTestOwner("Adult Owner", new DateTime(1980, 1, 1));
        var minorOwner = CreateTestOwner("Minor Owner", DateTime.Today.AddYears(-16));
        
        await _context.Owners.AddRangeAsync(adultOwner, minorOwner);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAdultOwnersAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(o => o.IdOwner == adultOwner.IdOwner);
        result.Should().NotContain(o => o.IdOwner == minorOwner.IdOwner);
    }

    [Test]
    public async Task GetOwnersByAgeRangeAsync_ValidRange_ShouldReturnOwnersInRange()
    {
        // Arrange
        var owner25 = CreateTestOwner("Owner 25", DateTime.Today.AddYears(-25));
        var owner35 = CreateTestOwner("Owner 35", DateTime.Today.AddYears(-35));
        var owner45 = CreateTestOwner("Owner 45", DateTime.Today.AddYears(-45));
        
        await _context.Owners.AddRangeAsync(owner25, owner35, owner45);
        await _context.SaveChangesAsync();

        // Act - Get owners between 30-40 years old
        var result = await _repository.GetOwnersByAgeRangeAsync(30, 40);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(o => o.IdOwner == owner35.IdOwner);
        result.Should().NotContain(o => o.IdOwner == owner25.IdOwner);
        result.Should().NotContain(o => o.IdOwner == owner45.IdOwner);
    }

    [Test]
    public async Task GetTotalPropertyCountAsync_OwnerWithProperties_ShouldReturnCorrectCount()
    {
        // Arrange
        var owner = CreateTestOwner("Owner With Properties");
        var property1 = CreateTestProperty("Property 1", owner);
        var property2 = CreateTestProperty("Property 2", owner);
        
        owner.AddProperty(property1.IdProperty);
        owner.AddProperty(property2.IdProperty);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTotalPropertyCountAsync(owner.IdOwner);

        // Assert
        result.Should().Be(2);
    }

    [Test]
    public async Task GetTotalPropertyCountAsync_OwnerWithoutProperties_ShouldReturnZero()
    {
        // Arrange
        var owner = CreateTestOwner("Owner Without Properties");
        
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTotalPropertyCountAsync(owner.IdOwner);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task GetTotalPropertyCountAsync_NonExistingOwner_ShouldReturnZero()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetTotalPropertyCountAsync(nonExistingId);

        // Assert
        result.Should().Be(0);
    }

    private Owner CreateTestOwner(string name, DateTime? birthDate = null)
    {
        var address = new Address("123 Test St", "Test City", "12345", "Test Country");
        var dateOfBirth = new DateOfBirth(birthDate ?? new DateTime(1980, 1, 1));
        return Owner.Create(name, address, dateOfBirth);
    }

    private Property CreateTestProperty(string name, Owner owner)
    {
        var address = new Address("456 Property St", "Property City", "67890", "Property Country");
        var price = new Money(200000, "USD");
        return Property.Create(name, address, price, $"CODE-{Guid.NewGuid().ToString("N")[..8]}", 2023, owner);
    }
}
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Persistance;

namespace Million.PropertiesService.Persistance.UnitTests.Configurations;

[TestFixture]
public class PropertyConfigurationTests
{
    private PropertiesDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PropertiesDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task PropertyConfiguration_ShouldPersistAndRetrieveCompleteProperty()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Test Street", "Test City", "12345", "Test Country");
        var price = new Money(250000.50m, "USD");
        var property = Property.Create("Test Property", address, price, "PROP-001", 2023, owner);

        await _context.Owners.AddAsync(owner);

        // Act
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Clear context to ensure we're reading from database
        _context.ChangeTracker.Clear();

        var retrievedProperty = await _context.Properties
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.IdProperty == property.IdProperty);

        // Assert
        retrievedProperty.Should().NotBeNull();
        retrievedProperty!.IdProperty.Should().Be(property.IdProperty);
        retrievedProperty.Name.Should().Be("Test Property");
        retrievedProperty.CodeInternal.Should().Be("PROP-001");
        retrievedProperty.Year.Should().Be(2023);
        retrievedProperty.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        
        // Test Address value object
        retrievedProperty.Address.Should().NotBeNull();
        retrievedProperty.Address.Street.Should().Be("123 Test Street");
        retrievedProperty.Address.City.Should().Be("Test City");
        retrievedProperty.Address.PostalCode.Should().Be("12345");
        retrievedProperty.Address.Country.Should().Be("Test Country");

        // Test Money value object
        retrievedProperty.Price.Should().NotBeNull();
        retrievedProperty.Price.Amount.Should().Be(250000.50m);
        retrievedProperty.Price.Currency.Should().Be("USD");

        // Test Owner relationship
        retrievedProperty.Owner.Should().NotBeNull();
        retrievedProperty.Owner.IdOwner.Should().Be(owner.IdOwner);
    }

    [Test]
    public async Task PropertyConfiguration_ShouldHandleNameMaxLength()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Test Street", "Test City", "12345", "Test Country");
        var price = new Money(200000, "USD");
        var longName = new string('A', 200); // Max length is 200
        var property = Property.Create(longName, address, price, "PROP-002", 2023, owner);

        await _context.Owners.AddAsync(owner);

        // Act & Assert
        await _context.Properties.AddAsync(property);
        await _context.Invoking(c => c.SaveChangesAsync()).Should().NotThrowAsync();
    }

    [Test]
    public async Task PropertyConfiguration_ShouldHandleCodeInternalMaxLength()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Test Street", "Test City", "12345", "Test Country");
        var price = new Money(200000, "USD");
        var longCode = new string('B', 50); // Max length is 50
        var property = Property.Create("Test Property", address, price, longCode, 2023, owner);

        await _context.Owners.AddAsync(owner);

        // Act & Assert
        await _context.Properties.AddAsync(property);
        await _context.Invoking(c => c.SaveChangesAsync()).Should().NotThrowAsync();
    }


    [Test]
    public async Task PropertyConfiguration_ShouldHandleCascadeDeleteForPropertyImages()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Test Street", "Test City", "12345", "Test Country");
        var price = new Money(200000, "USD");
        var property = Property.Create("Test Property", address, price, "PROP-004", 2023, owner);

        var propertyImage = PropertyImage.Create(property.IdProperty, "test.jpg", true);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddAsync(propertyImage);
        await _context.SaveChangesAsync();

        // Act - Delete the property
        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        // Assert - PropertyImage should be deleted due to cascade
        var remainingImages = await _context.PropertyImages
            .Where(pi => pi.IdProperty == property.IdProperty)
            .ToListAsync();
        remainingImages.Should().BeEmpty();
    }

    [Test]
    public async Task PropertyConfiguration_ShouldHandleCascadeDeleteForPropertyTraces()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Test Street", "Test City", "12345", "Test Country");
        var price = new Money(200000, "USD");
        var property = Property.Create("Test Property", address, price, "PROP-005", 2023, owner);

        var propertyTrace = PropertyTrace.Create(property.IdProperty, new Money(210000, "USD"), 5.0m);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddAsync(propertyTrace);
        await _context.SaveChangesAsync();

        // Act - Delete the property
        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        // Assert - PropertyTrace should be deleted due to cascade
        var remainingTraces = await _context.PropertyTraces
            .Where(pt => pt.IdProperty == property.IdProperty)
            .ToListAsync();
        remainingTraces.Should().BeEmpty();
    }

    [Test]
    public async Task PropertyConfiguration_ShouldRestrictDeleteWhenOwnerIsReferenced()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Test Street", "Test City", "12345", "Test Country");
        var price = new Money(200000, "USD");
        var property = Property.Create("Test Property", address, price, "PROP-006", 2023, owner);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act & Assert - Try to delete the owner while property exists
        _context.Owners.Remove(owner);
        
        // This should fail due to foreign key constraint (Restrict behavior)
        var exception = await _context.Invoking(c => c.SaveChangesAsync())
            .Should().ThrowAsync<Exception>();
    }

    [Test]
    public async Task PropertyConfiguration_ShouldPersistUpdatedAt()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Test Street", "Test City", "12345", "Test Country");
        var price = new Money(200000, "USD");
        var property = Property.Create("Original Name", address, price, "PROP-007", 2023, owner);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act - Update the property
        property.UpdateName("Updated Name");
        await _context.SaveChangesAsync();

        // Clear context to ensure we're reading from database
        _context.ChangeTracker.Clear();

        var retrievedProperty = await _context.Properties
            .FirstOrDefaultAsync(p => p.IdProperty == property.IdProperty);

        // Assert
        retrievedProperty.Should().NotBeNull();
        retrievedProperty!.Name.Should().Be("Updated Name");
        retrievedProperty.UpdatedAt.Should().NotBeNull();
        retrievedProperty.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    private Owner CreateTestOwner()
    {
        var address = new Address("456 Owner Street", "Owner City", "67890", "Owner Country");
        var dateOfBirth = new DateOfBirth(new DateTime(1980, 1, 1));
        return Owner.Create("Test Owner", address, dateOfBirth);
    }
}
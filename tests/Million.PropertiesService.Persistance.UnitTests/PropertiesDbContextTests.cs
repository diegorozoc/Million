using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;

namespace Million.PropertiesService.Persistance.UnitTests;

[TestFixture]
public class PropertiesDbContextTests
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
    public void Constructor_WithValidOptions_ShouldCreateContext()
    {
        // Act & Assert
        _context.Should().NotBeNull();
        _context.Database.Should().NotBeNull();
    }

    [Test]
    public void DbSets_ShouldBeInitialized()
    {
        // Act & Assert
        _context.Properties.Should().NotBeNull();
        _context.PropertyImages.Should().NotBeNull();
        _context.PropertyTraces.Should().NotBeNull();
        _context.Owners.Should().NotBeNull();
    }

    [Test]
    public async Task DbContext_ShouldPersistAllEntities()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var propertyImage = PropertyImage.Create(property.IdProperty, "test.jpg", true);
        var propertyTrace = PropertyTrace.Create(property.IdProperty, new Money(220000, "USD"), 5.0m);

        // Act
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddAsync(propertyImage);
        await _context.PropertyTraces.AddAsync(propertyTrace);
        await _context.SaveChangesAsync();

        // Assert
        var savedOwner = await _context.Owners.FindAsync(owner.IdOwner);
        var savedProperty = await _context.Properties.FindAsync(property.IdProperty);
        var savedImage = await _context.PropertyImages.FindAsync(propertyImage.IdPropertyImage);
        var savedTrace = await _context.PropertyTraces.FindAsync(propertyTrace.IdPropertyTrace);

        savedOwner.Should().NotBeNull();
        savedProperty.Should().NotBeNull();
        savedImage.Should().NotBeNull();
        savedTrace.Should().NotBeNull();
    }

    [Test]
    public async Task DbContext_ShouldApplyDefaultSchema()
    {
        // Arrange
        var owner = CreateTestOwner();

        // Act
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        // Assert - This test verifies that the context doesn't throw errors when applying schema
        var savedOwner = await _context.Owners.FindAsync(owner.IdOwner);
        savedOwner.Should().NotBeNull();
    }

    [Test]
    public async Task DbContext_ShouldHandleEntityRelationships()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);

        // Act
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Clear context to test loading from database
        _context.ChangeTracker.Clear();

        // Assert
        var loadedProperty = await _context.Properties
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.IdProperty == property.IdProperty);

        loadedProperty.Should().NotBeNull();
        loadedProperty!.Owner.Should().NotBeNull();
        loadedProperty.Owner.IdOwner.Should().Be(owner.IdOwner);
        loadedProperty.IdOwner.Should().Be(owner.IdOwner);
    }

    [Test]
    public async Task DbContext_ShouldHandleValueObjects()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("123 Complex Street", "Complex City", "54321", "Complex Country");
        var price = new Money(350000.99m, "EUR");
        var property = Property.Create("Value Object Test", address, price, "VO-001", 2023, owner);

        // Act
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Clear context to test loading from database
        _context.ChangeTracker.Clear();

        // Assert
        var loadedProperty = await _context.Properties
            .FirstOrDefaultAsync(p => p.IdProperty == property.IdProperty);

        loadedProperty.Should().NotBeNull();
        
        // Test Address value object
        loadedProperty!.Address.Should().NotBeNull();
        loadedProperty.Address.Street.Should().Be("123 Complex Street");
        loadedProperty.Address.City.Should().Be("Complex City");
        loadedProperty.Address.PostalCode.Should().Be("54321");
        loadedProperty.Address.Country.Should().Be("Complex Country");

        // Test Money value object
        loadedProperty.Price.Should().NotBeNull();
        loadedProperty.Price.Amount.Should().Be(350000.99m);
        loadedProperty.Price.Currency.Should().Be("EUR");
    }

    [Test]
    public async Task DbContext_ShouldHandleConcurrency()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Concurrent Test", owner);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act - Simulate concurrent access
        using var context1 = CreateNewContext();
        using var context2 = CreateNewContext();

        var property1 = await context1.Properties.FindAsync(property.IdProperty);
        var property2 = await context2.Properties.FindAsync(property.IdProperty);

        property1!.UpdateName("Updated by Context 1");
        property2!.UpdateName("Updated by Context 2");

        await context1.SaveChangesAsync();
        
        // Act & Assert - Second context should save successfully (Last Write Wins)
        await context2.Invoking(c => c.SaveChangesAsync()).Should().NotThrowAsync();
    }

    [Test]
    public async Task DbContext_ShouldHandleTransactions()
    {
        // Arrange
        var owner1 = CreateTestOwner("Owner 1");
        var owner2 = CreateTestOwner("Owner 2");

        // Act
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            await _context.Owners.AddAsync(owner1);
            await _context.SaveChangesAsync();
            
            await _context.Owners.AddAsync(owner2);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Assert
        var savedOwner1 = await _context.Owners.FindAsync(owner1.IdOwner);
        var savedOwner2 = await _context.Owners.FindAsync(owner2.IdOwner);

        savedOwner1.Should().NotBeNull();
        savedOwner2.Should().NotBeNull();
    }

    [Test]
    public async Task DbContext_ShouldTrackChanges()
    {
        // Arrange
        var owner = CreateTestOwner();
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        // Act
        owner.UpdateName("Updated Name");

        // Assert
        _context.Entry(owner).State.Should().Be(EntityState.Modified);
        
        await _context.SaveChangesAsync();
        _context.Entry(owner).State.Should().Be(EntityState.Unchanged);
    }

    [Test]
    public void DbContext_Model_ShouldContainAllEntityTypes()
    {
        // Act
        var model = _context.Model;

        // Assert
        var entityTypes = model.GetEntityTypes().Select(et => et.ClrType).ToList();
        
        entityTypes.Should().Contain(typeof(Property));
        entityTypes.Should().Contain(typeof(PropertyImage));
        entityTypes.Should().Contain(typeof(PropertyTrace));
        entityTypes.Should().Contain(typeof(Owner));
    }

    private PropertiesDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseInMemoryDatabase(databaseName: _context.Database.GetDbConnection().Database)
            .Options;

        return new PropertiesDbContext(options);
    }

    private Owner CreateTestOwner(string name = "Test Owner")
    {
        var address = new Address("123 Test St", "Test City", "12345", "Test Country");
        var dateOfBirth = new DateOfBirth(new DateTime(1980, 1, 1));
        return Owner.Create(name, address, dateOfBirth);
    }

    private Property CreateTestProperty(string name, Owner owner)
    {
        var address = new Address("456 Property St", "Property City", "67890", "Property Country");
        var price = new Money(200000, "USD");
        return Property.Create(name, address, price, $"CODE-{Guid.NewGuid().ToString("N")[..8]}", 2023, owner);
    }
}
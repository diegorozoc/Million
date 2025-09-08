using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Common.Specifications;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Specifications;
using Million.PropertiesService.Persistance;
using Million.PropertiesService.Persistance.Properties.Repositories;

namespace Million.PropertiesService.Persistance.UnitTests.Repositories;

[TestFixture]
public class PropertyRepositoryTests
{
    private PropertiesDbContext _context = null!;
    private PropertyRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PropertiesDbContext(options);
        _repository = new PropertyRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_ExistingProperty_ShouldReturnProperty()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(property.IdProperty);

        // Assert
        result.Should().NotBeNull();
        result!.IdProperty.Should().Be(property.IdProperty);
        result.Name.Should().Be("Test Property");
    }

    [Test]
    public async Task GetByIdAsync_NonExistingProperty_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task SaveAsync_NewProperty_ShouldAddProperty()
    {
        // Arrange
        var owner = CreateTestOwner();
        await _context.Owners.AddAsync(owner);
        await _context.SaveChangesAsync();

        var property = CreateTestProperty("New Property", owner);

        // Act
        await _repository.SaveAsync(property);

        // Assert
        var savedProperty = await _context.Properties.FindAsync(property.IdProperty);
        savedProperty.Should().NotBeNull();
        savedProperty!.Name.Should().Be("New Property");
    }

    [Test]
    public async Task SaveAsync_ExistingProperty_ShouldUpdateProperty()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Original Name", owner);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        property.UpdateName("Updated Name");

        // Act
        await _repository.SaveAsync(property);

        // Assert
        var updatedProperty = await _context.Properties.FindAsync(property.IdProperty);
        updatedProperty.Should().NotBeNull();
        updatedProperty!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task DeleteAsync_ExistingProperty_ShouldRemoveProperty()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Property to Delete", owner);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(property.IdProperty);

        // Assert
        var deletedProperty = await _context.Properties.FindAsync(property.IdProperty);
        deletedProperty.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_NonExistingProperty_ShouldNotThrow()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await _repository.Invoking(r => r.DeleteAsync(nonExistingId))
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task ExistsAsync_ExistingProperty_ShouldReturnTrue()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Existing Property", owner);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(property.IdProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_NonExistingProperty_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistingId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task CodeInternalExistsAsync_ExistingCode_ShouldReturnTrue()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner, "UNIQUE-CODE-001");
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CodeInternalExistsAsync("UNIQUE-CODE-001");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task CodeInternalExistsAsync_NonExistingCode_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.CodeInternalExistsAsync("NON-EXISTING-CODE");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task FindAsync_WithSpecification_ShouldReturnMatchingProperties()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("Property 1", owner, "CODE-001", 2020);
        var property2 = CreateTestProperty("Property 2", owner, "CODE-002", 2021);
        var property3 = CreateTestProperty("Property 3", owner, "CODE-003", 2019);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2, property3);
        await _context.SaveChangesAsync();

        var specification = new PropertyFilterSpecification(year: 2020);

        // Act
        var result = await _repository.FindAsync(specification);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.IdProperty == property1.IdProperty);
    }

    [Test]
    public async Task FindOneAsync_WithSpecification_ShouldReturnFirstMatch()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("First Match", owner, "CODE-001", 2020);
        var property2 = CreateTestProperty("Second Match", owner, "CODE-002", 2020);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2);
        await _context.SaveChangesAsync();

        var specification = new PropertyFilterSpecification(year: 2020);

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        result.Should().NotBeNull();
        result!.Year.Should().Be(2020);
    }

    [Test]
    public async Task CountAsync_WithSpecification_ShouldReturnCorrectCount()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("Property 1", owner, "CODE-001", 2020);
        var property2 = CreateTestProperty("Property 2", owner, "CODE-002", 2020);
        var property3 = CreateTestProperty("Property 3", owner, "CODE-003", 2019);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2, property3);
        await _context.SaveChangesAsync();

        var specification = new PropertyFilterSpecification(year: 2020);

        // Act
        var result = await _repository.CountAsync(specification);

        // Assert
        result.Should().Be(2);
    }

    private Owner CreateTestOwner()
    {
        var address = new Address("123 Test St", "Test City", "12345", "Test Country");
        var dateOfBirth = new DateOfBirth(new DateTime(1980, 1, 1));
        return Owner.Create("Test Owner", address, dateOfBirth);
    }

    private Property CreateTestProperty(string name, Owner owner, string codeInternal = "TEST-CODE", int year = 2023)
    {
        var address = new Address("456 Property St", "Property City", "67890", "Property Country");
        var price = new Money(200000, "USD");
        return Property.Create(name, address, price, codeInternal, year, owner);
    }
}
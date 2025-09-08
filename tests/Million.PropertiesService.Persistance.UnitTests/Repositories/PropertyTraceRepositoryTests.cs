using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Specifications;
using Million.PropertiesService.Persistance;
using Million.PropertiesService.Persistance.Properties.Repositories;

namespace Million.PropertiesService.Persistance.UnitTests.Repositories;

[TestFixture]
public class PropertyTraceRepositoryTests
{
    private PropertiesDbContext _context = null!;
    private PropertyTraceRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PropertiesDbContext(options);
        _repository = new PropertyTraceRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_ExistingTrace_ShouldReturnTrace()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace = PropertyTrace.Create(property.IdProperty, new Money(250000, "USD"), 5.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddAsync(trace);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(trace.IdPropertyTrace);

        // Assert
        result.Should().NotBeNull();
        result!.IdPropertyTrace.Should().Be(trace.IdPropertyTrace);
        result.Value.Amount.Should().Be(250000);
        result.TaxPercentage.Should().Be(5.0m);
    }

    [Test]
    public async Task GetByIdAsync_NonExistingTrace_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task SaveAsync_NewTrace_ShouldAddTrace()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace = PropertyTrace.Create(property.IdProperty, new Money(300000, "USD"), 6.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        await _repository.SaveAsync(trace);

        // Assert
        var savedTrace = await _context.PropertyTraces.FindAsync(trace.IdPropertyTrace);
        savedTrace.Should().NotBeNull();
        savedTrace!.Value.Amount.Should().Be(300000);
        savedTrace.TaxPercentage.Should().Be(6.0m);
    }

    [Test]
    public async Task DeleteAsync_ExistingTrace_ShouldRemoveTrace()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace = PropertyTrace.Create(property.IdProperty, new Money(200000, "USD"), 4.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddAsync(trace);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(trace.IdPropertyTrace);

        // Assert
        var deletedTrace = await _context.PropertyTraces.FindAsync(trace.IdPropertyTrace);
        deletedTrace.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_NonExistingTrace_ShouldNotThrow()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await _repository.Invoking(r => r.DeleteAsync(nonExistingId))
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task ExistsAsync_ExistingTrace_ShouldReturnTrue()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace = PropertyTrace.Create(property.IdProperty, new Money(150000, "USD"), 3.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddAsync(trace);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(trace.IdPropertyTrace);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_NonExistingTrace_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistingId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task FindAsync_WithSpecification_ShouldReturnMatchingTraces()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace1 = PropertyTrace.Create(property.IdProperty, new Money(200000, "USD"), 5.0m);
        var trace2 = PropertyTrace.Create(property.IdProperty, new Money(250000, "USD"), 7.0m);
        var trace3 = PropertyTrace.Create(property.IdProperty, new Money(180000, "USD"), 3.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddRangeAsync(trace1, trace2, trace3);
        await _context.SaveChangesAsync();

        var specification = new PropertyTracesByPropertyIdSpecification(property.IdProperty);

        // Act
        var result = await _repository.FindAsync(specification);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.IdPropertyTrace == trace1.IdPropertyTrace);
        result.Should().Contain(t => t.IdPropertyTrace == trace2.IdPropertyTrace);
        result.Should().Contain(t => t.IdPropertyTrace == trace3.IdPropertyTrace);
    }

    [Test]
    public async Task FindOneAsync_WithSpecification_ShouldReturnFirstMatch()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace = PropertyTrace.Create(property.IdProperty, new Money(200000, "USD"), 5.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddAsync(trace);
        await _context.SaveChangesAsync();

        var specification = new PropertyTracesByPropertyIdSpecification(property.IdProperty);

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        result.Should().NotBeNull();
        result!.IdPropertyTrace.Should().Be(trace.IdPropertyTrace);
    }

    [Test]
    public async Task CountAsync_WithSpecification_ShouldReturnCorrectCount()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("Property 1", owner);
        var property2 = CreateTestProperty("Property 2", owner);
        var trace1 = PropertyTrace.Create(property1.IdProperty, new Money(200000, "USD"), 5.0m);
        var trace2 = PropertyTrace.Create(property1.IdProperty, new Money(220000, "USD"), 5.5m);
        var trace3 = PropertyTrace.Create(property2.IdProperty, new Money(300000, "USD"), 6.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2);
        await _context.PropertyTraces.AddRangeAsync(trace1, trace2, trace3);
        await _context.SaveChangesAsync();

        var specification = new PropertyTracesByPropertyIdSpecification(property1.IdProperty);

        // Act
        var result = await _repository.CountAsync(specification);

        // Assert
        result.Should().Be(2);
    }

    [Test]
    public async Task PropertyHasTracesAsync_PropertyWithTraces_ShouldReturnTrue()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace = PropertyTrace.Create(property.IdProperty, new Money(200000, "USD"), 5.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddAsync(trace);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PropertyHasTracesAsync(property.IdProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task PropertyHasTracesAsync_PropertyWithoutTraces_ShouldReturnFalse()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PropertyHasTracesAsync(property.IdProperty);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetTraceCountByPropertyAsync_MultipleTraces_ShouldReturnCorrectCount()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace1 = PropertyTrace.Create(property.IdProperty, new Money(200000, "USD"), 5.0m);
        var trace2 = PropertyTrace.Create(property.IdProperty, new Money(210000, "USD"), 5.2m);
        var trace3 = PropertyTrace.Create(property.IdProperty, new Money(220000, "USD"), 5.5m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddRangeAsync(trace1, trace2, trace3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTraceCountByPropertyAsync(property.IdProperty);

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public async Task GetAverageValueByPropertyAsync_MultipleTraces_ShouldReturnCorrectAverage()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace1 = PropertyTrace.Create(property.IdProperty, new Money(200000, "USD"), 5.0m);
        var trace2 = PropertyTrace.Create(property.IdProperty, new Money(300000, "USD"), 6.0m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddRangeAsync(trace1, trace2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAverageValueByPropertyAsync(property.IdProperty);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(250000); // (200000 + 300000) / 2
        result.Currency.Should().Be("USD");
    }

    [Test]
    public async Task GetAverageValueByPropertyAsync_NoTraces_ShouldReturnNull()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAverageValueByPropertyAsync(property.IdProperty);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task DeleteByPropertyIdAsync_MultipleTraces_ShouldDeleteAllPropertyTraces()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var trace1 = PropertyTrace.Create(property.IdProperty, new Money(200000, "USD"), 5.0m);
        var trace2 = PropertyTrace.Create(property.IdProperty, new Money(220000, "USD"), 5.5m);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyTraces.AddRangeAsync(trace1, trace2);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteByPropertyIdAsync(property.IdProperty);

        // Assert
        var remainingTraces = await _context.PropertyTraces
            .Where(pt => pt.IdProperty == property.IdProperty)
            .ToListAsync();
        remainingTraces.Should().BeEmpty();
    }

    private Owner CreateTestOwner()
    {
        var address = new Address("123 Test St", "Test City", "12345", "Test Country");
        var dateOfBirth = new DateOfBirth(new DateTime(1980, 1, 1));
        return Owner.Create("Test Owner", address, dateOfBirth);
    }

    private Property CreateTestProperty(string name, Owner owner)
    {
        var address = new Address("456 Property St", "Property City", "67890", "Property Country");
        var price = new Money(200000, "USD");
        return Property.Create(name, address, price, $"CODE-{Guid.NewGuid().ToString("N")[..8]}", 2023, owner);
    }
}
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Common.Specifications;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Specifications;
using Million.PropertiesService.Persistance;
using Million.PropertiesService.Persistance.Common;
using System.Linq.Expressions;

namespace Million.PropertiesService.Persistance.UnitTests.Common;

[TestFixture]
public class SpecificationEvaluatorTests
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
    public async Task GetQuery_WithCriteria_ShouldApplyWhereClause()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("Property 2020", owner, 2020);
        var property2 = CreateTestProperty("Property 2021", owner, 2021);
        var property3 = CreateTestProperty("Property 2019", owner, 2019);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2, property3);
        await _context.SaveChangesAsync();

        var specification = new TestSpecification(p => p.Year >= 2020);

        // Act
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.IdProperty == property1.IdProperty);
        result.Should().Contain(p => p.IdProperty == property2.IdProperty);
        result.Should().NotContain(p => p.IdProperty == property3.IdProperty);
    }

    [Test]
    public async Task GetQuery_WithOrderBy_ShouldApplyOrdering()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("Property C", owner, 2020);
        var property2 = CreateTestProperty("Property A", owner, 2021);
        var property3 = CreateTestProperty("Property B", owner, 2019);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2, property3);
        await _context.SaveChangesAsync();

        var specification = new TestSpecification(orderBy: p => p.Name);

        // Act
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Property A");
        result[1].Name.Should().Be("Property B");
        result[2].Name.Should().Be("Property C");
    }

    [Test]
    public async Task GetQuery_WithOrderByDescending_ShouldApplyDescendingOrdering()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("Property", owner, 2019);
        var property2 = CreateTestProperty("Property", owner, 2021);
        var property3 = CreateTestProperty("Property", owner, 2020);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2, property3);
        await _context.SaveChangesAsync();

        var specification = new TestSpecification(orderByDescending: p => p.Year);

        // Act
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Year.Should().Be(2021);
        result[1].Year.Should().Be(2020);
        result[2].Year.Should().Be(2019);
    }

    [Test]
    public async Task GetQuery_WithPaging_ShouldApplySkipAndTake()
    {
        // Arrange
        var owner = CreateTestOwner();
        var properties = new List<Property>();
        
        for (int i = 1; i <= 10; i++)
        {
            properties.Add(CreateTestProperty($"Property {i:D2}", owner, 2020 + i));
        }

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(properties);
        await _context.SaveChangesAsync();

        var specification = new TestSpecification(
            orderBy: p => p.Name,
            skip: 3,
            take: 4);

        // Act
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(4);
        result[0].Name.Should().Be("Property 04");
        result[1].Name.Should().Be("Property 05");
        result[2].Name.Should().Be("Property 06");
        result[3].Name.Should().Be("Property 07");
    }

    [Test]
    public async Task GetQuery_WithComplexSpecification_ShouldApplyAllConditions()
    {
        // Arrange
        var owner = CreateTestOwner();
        var properties = new List<Property>();
        
        for (int i = 2015; i <= 2025; i++)
        {
            properties.Add(CreateTestProperty($"Property {i}", owner, i));
        }

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(properties);
        await _context.SaveChangesAsync();

        var specification = new TestSpecification(
            criteria: p => p.Year >= 2020 && p.Year <= 2023,
            orderByDescending: p => p.Year,
            skip: 1,
            take: 2);

        // Act
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Year.Should().Be(2022); // Second item after ordering by year desc
        result[1].Year.Should().Be(2021); // Third item after ordering by year desc
    }

    [Test]
    public async Task GetQuery_WithNoSpecificationConditions_ShouldReturnAllItems()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property1 = CreateTestProperty("Property 1", owner, 2020);
        var property2 = CreateTestProperty("Property 2", owner, 2021);

        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddRangeAsync(property1, property2);
        await _context.SaveChangesAsync();

        var specification = new TestSpecification(); // Empty specification

        // Act
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Test]
    public void GetQuery_WithRealPropertyFilterSpecification_ShouldWork()
    {
        // Arrange
        var query = _context.Properties.AsQueryable();
        var specification = new PropertyFilterSpecification(year: 2020);

        // Act & Assert
        var result = SpecificationEvaluator.GetQuery(query, specification);
        result.Should().NotBeNull();
        result.Should().NotBeNull();
    }

    private Owner CreateTestOwner()
    {
        var address = new Address("123 Test St", "Test City", "12345", "Test Country");
        var dateOfBirth = new DateOfBirth(new DateTime(1980, 1, 1));
        return Owner.Create("Test Owner", address, dateOfBirth);
    }

    private Property CreateTestProperty(string name, Owner owner, int year = 2023)
    {
        var address = new Address("456 Property St", "Property City", "67890", "Property Country");
        var price = new Money(200000, "USD");
        return Property.Create(name, address, price, $"CODE-{Guid.NewGuid().ToString("N")[..8]}", year, owner);
    }

    // Test specification implementation
    private class TestSpecification : BaseSpecification<Property>
    {
        public TestSpecification(
            Expression<Func<Property, bool>>? criteria = null,
            Expression<Func<Property, object>>? orderBy = null,
            Expression<Func<Property, object>>? orderByDescending = null,
            int? skip = null,
            int? take = null) : base(criteria ?? (p => true))
        {
            if (orderBy != null)
                ApplyOrderBy(orderBy);
                
            if (orderByDescending != null)
                ApplyOrderByDescending(orderByDescending);
                
            if (skip.HasValue || take.HasValue)
            {
                ApplyPaging(skip ?? 0, take ?? int.MaxValue);
            }
        }
    }
}
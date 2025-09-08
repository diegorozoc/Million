using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Million.PropertiesService.Api.Models;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesService.IntegrationTests.Infrastructure;
using Million.PropertiesService.Persistance;
using Million.PropertiesServices.Application.Properties.Models;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.IntegrationTests.Controllers;

[TestFixture]
public class PropertiesControllerIntegrationTests : IntegrationTestBase
{
    private string _authToken = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        _authToken = await GetJwtTokenAsync();
        SetAuthorizationHeader(_authToken);
    }

    #region CreateProperty Integration Tests

    [Test]
    public async Task CreateProperty_WithValidData_ShouldCreatePropertyInDatabase()
    {
        // Arrange
        var owner = CreateTestOwner();
        var address = new Address("456 Integration St", "Integration City", "54321", "Integration Country");
        var price = new Money(750000m, "USD");
        
        var newProperty = new NewProperty(
            "Integration Test Property",
            address,
            price,
            "INT-001",
            2024,
            owner.IdOwner
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/properties", newProperty);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdProperty = await DeserializeResponse<CreatePropertyBuildingResponse>(response);
        createdProperty.Should().NotBeNull();
        createdProperty!.IdProperty.Should().NotBeEmpty();

        // Verify in database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
        var propertyInDb = await dbContext.Properties
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.IdProperty == createdProperty.IdProperty);
        
        propertyInDb.Should().NotBeNull();
        propertyInDb!.Name.Should().Be("Integration Test Property");
        propertyInDb.Address.Street.Should().Be("456 Integration St");
        propertyInDb.Address.City.Should().Be("Integration City");
        propertyInDb.Address.PostalCode.Should().Be("54321");
        propertyInDb.Address.Country.Should().Be("Integration Country");
        propertyInDb.Price.Amount.Should().Be(750000m);
        propertyInDb.Price.Currency.Should().Be("USD");
        propertyInDb.CodeInternal.Should().Be("INT-001");
        propertyInDb.Year.Should().Be(2024);
        propertyInDb.IdOwner.Should().Be(owner.IdOwner);
        propertyInDb.Owner.Should().NotBeNull();
    }

    [Test]
    public async Task CreateProperty_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var owner = CreateTestOwner();
        var address = new Address("123 Test St", "Test City", "12345", "Test Country");
        var price = new Money(500000m, "USD");
        
        var newProperty = new NewProperty(
            "Test Property",
            address,
            price,
            "TEST-001",
            2023,
            owner.IdOwner
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/properties", newProperty);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region AddPropertyImage Integration Tests

    [Test]
    public async Task AddPropertyImage_WithValidData_ShouldCreateImageInDatabase()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = await CreateTestPropertyAsync(owner.IdOwner);
        
        var newPropertyImage = new NewPropertyImage(
            "test-image.jpg",
            true,
            property.IdProperty
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/properties/images", newPropertyImage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdImage = await DeserializeResponse<AddImageFromPropertyResponse>(response);
        createdImage.Should().NotBeNull();
        createdImage!.IdPropertyImage.Should().NotBeEmpty();

        // Verify in database
        using var scope3 = Factory.Services.CreateScope();
        var dbContext3 = scope3.ServiceProvider.GetRequiredService<PropertiesDbContext>();
        var imageInDb = await dbContext3.PropertyImages
            .FirstOrDefaultAsync(i => i.IdPropertyImage == createdImage.IdPropertyImage);
        
        imageInDb.Should().NotBeNull();
        imageInDb!.FileName.Should().Be("test-image.jpg");
        imageInDb.Enabled.Should().BeTrue();
        imageInDb.IdProperty.Should().Be(property.IdProperty);
    }

    [Test]
    public async Task AddPropertyImage_WithInvalidProperty_ShouldThrowValidationException()
    {
        // Arrange
        var invalidPropertyId = Guid.NewGuid();
        var newPropertyImage = new NewPropertyImage(
            "test-image.jpg",
            true,
            invalidPropertyId
        );

        // Act & Assert - This operation should fail with validation exception
        try
        {
            var response = await Client.PostAsJsonAsync("/api/v1/properties/images", newPropertyImage);
            // If we get here without exception, the test should fail
            Assert.Fail("Expected ValidationException but none was thrown");
        }
        catch (HttpRequestException ex)
        {
            // This is expected - the validation exception gets wrapped in HttpRequestException
            ex.Message.Should().Contain("500");
        }
        catch (Exception ex)
        {
            // Check if it's our expected validation exception
            ex.Should().BeOfType<FluentValidation.ValidationException>()
                .Which.Message.Should().Contain("Property with ID");
        }
        
        // Verify no image was created
        using var scope4 = Factory.Services.CreateScope();
        var dbContext4 = scope4.ServiceProvider.GetRequiredService<PropertiesDbContext>();
        var imagesCount = await dbContext4.PropertyImages.CountAsync();
        imagesCount.Should().Be(0);
    }

    #endregion

    #region ChangePropertyPrice Integration Tests

    [Test]
    public async Task ChangePropertyPrice_WithValidData_ShouldUpdatePriceInDatabase()
    {
        // Arrange - Get admin token for price changes
        var adminToken = await GetJwtTokenAsync("admin@million.com", "admin123");
        SetAuthorizationHeader(adminToken);
        
        var owner = CreateTestOwner();
        var property = await CreateTestPropertyAsync(owner.IdOwner);
        var originalPrice = property.Price.Amount;
        
        var newPrice = new Money(900000m, "USD");
        var changePriceRequest = new ChangePrice(newPrice);

        // Act
        var response = await PutAsJsonAsync($"/api/v1/properties/{property.IdProperty}/price", changePriceRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify in database
        using var scope5 = Factory.Services.CreateScope();
        var dbContext5 = scope5.ServiceProvider.GetRequiredService<PropertiesDbContext>();
        var updatedProperty = await dbContext5.Properties
            .FirstOrDefaultAsync(p => p.IdProperty == property.IdProperty);
        
        updatedProperty.Should().NotBeNull();
        updatedProperty!.Price.Amount.Should().Be(900000m);
        updatedProperty.Price.Currency.Should().Be("USD");
        updatedProperty.Price.Amount.Should().NotBe(originalPrice);
        updatedProperty.UpdatedAt.Should().NotBeNull();
    }


    [Test]
    public async Task ChangePropertyPrice_WithManagerRole_ShouldReturnForbidden()
    {
        // Arrange - Get manager token (only admin can change prices)
        var managerToken = await GetJwtTokenAsync("manager@million.com", "manager123");
        SetAuthorizationHeader(managerToken);
        
        var owner = CreateTestOwner();
        var property = await CreateTestPropertyAsync(owner.IdOwner);
        var newPrice = new Money(900000m, "USD");
        var changePriceRequest = new ChangePrice(newPrice);

        // Act
        var response = await PutAsJsonAsync($"/api/v1/properties/{property.IdProperty}/price", changePriceRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region UpdateProperty Integration Tests

    [Test]
    public async Task UpdateProperty_WithValidData_ShouldUpdatePropertyInDatabase()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = await CreateTestPropertyAsync(owner.IdOwner);
        var newOwner = CreateTestOwner("New Owner", "456 New St", "New City");
        
        var updateRequest = new UpdatePropertyRequest(
            "Updated Property Name",
            new Address("789 Updated St", "Updated City", "98765", "Updated Country"),
            2025,
            newOwner.IdOwner
        );

        // Act
        var response = await PutAsJsonAsync($"/api/v1/properties/{property.IdProperty}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify in database
        using var scope6 = Factory.Services.CreateScope();
        var dbContext6 = scope6.ServiceProvider.GetRequiredService<PropertiesDbContext>();
        var updatedProperty = await dbContext6.Properties
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.IdProperty == property.IdProperty);
        
        updatedProperty.Should().NotBeNull();
        updatedProperty!.Name.Should().Be("Updated Property Name");
        updatedProperty.Address.Street.Should().Be("789 Updated St");
        updatedProperty.Address.City.Should().Be("Updated City");
        updatedProperty.Address.PostalCode.Should().Be("98765");
        updatedProperty.Address.Country.Should().Be("Updated Country");
        updatedProperty.Year.Should().Be(2025);
        updatedProperty.IdOwner.Should().Be(newOwner.IdOwner);
        updatedProperty.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region GetProperties Integration Tests

    [Test]
    public async Task GetProperties_WithNoFilters_ShouldReturnAllPropertiesFromDatabase()
    {
        // Arrange
        var owner1 = CreateTestOwner("Owner 1");
        var owner2 = CreateTestOwner("Owner 2");
        var property1 = await CreateTestPropertyAsync(owner1.IdOwner, "Property 1", "USA", "New York", 500000m, 2023);
        var property2 = await CreateTestPropertyAsync(owner2.IdOwner, "Property 2", "Canada", "Toronto", 750000m, 2024);

        // Act
        var response = await Client.GetAsync("/api/v1/properties");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var properties = await response.Content.ReadFromJsonAsync<List<GetPropertiesResponse>>();
        properties.Should().NotBeNull();
        properties!.Count.Should().Be(2);
        
        properties.Should().Contain(p => p.IdProperty == property1.IdProperty);
        properties.Should().Contain(p => p.IdProperty == property2.IdProperty);
    }

    [Test]
    public async Task GetProperties_WithCountryFilter_ShouldReturnFilteredPropertiesFromDatabase()
    {
        // Arrange
        var owner1 = CreateTestOwner("Owner 1");
        var usaProperty = await CreateTestPropertyAsync(owner1.IdOwner, "USA Property", "USA", "New York", 500000m, 2023);

        // Act
        var response = await Client.GetAsync("/api/v1/properties?country=USA");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var properties = await response.Content.ReadFromJsonAsync<List<GetPropertiesResponse>>();
        properties.Should().NotBeNull();
        properties!.Count.Should().Be(1);
        properties[0].IdProperty.Should().Be(usaProperty.IdProperty);
        properties[0].Address.Country.Should().Be("USA");
    }

    [Test]
    public async Task GetProperties_WithPriceRangeFilter_ShouldReturnFilteredPropertiesFromDatabase()
    {
        // Arrange
        var owner1 = CreateTestOwner("Owner 1");
        var owner2 = CreateTestOwner("Owner 2");
        await CreateTestPropertyAsync(owner1.IdOwner, "Cheap Property", "USA", "City1", 300000m, 2023);
        await CreateTestPropertyAsync(owner2.IdOwner, "Expensive Property", "USA", "City2", 800000m, 2024);

        // Act
        var response = await Client.GetAsync("/api/v1/properties?minPrice=400000&maxPrice=700000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var properties = await response.Content.ReadFromJsonAsync<List<GetPropertiesResponse>>();
        properties.Should().NotBeNull();
        properties!.Count.Should().Be(0); // No properties in the 400k-700k range
    }

    [Test]
    public async Task GetProperties_WithMultipleFilters_ShouldReturnFilteredPropertiesFromDatabase()
    {
        // Arrange
        var owner1 = CreateTestOwner("Owner 1");
        var owner2 = CreateTestOwner("Owner 2");
        var matchingProperty = await CreateTestPropertyAsync(owner1.IdOwner, "Matching Property", "USA", "New York", 500000m, 2023);
        await CreateTestPropertyAsync(owner2.IdOwner, "Non-Matching Property", "Canada", "Toronto", 500000m, 2024);

        // Act
        var response = await Client.GetAsync("/api/v1/properties?country=USA&city=New York&year=2023");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var properties = await response.Content.ReadFromJsonAsync<List<GetPropertiesResponse>>();
        properties.Should().NotBeNull();
        properties!.Count.Should().Be(1);
        properties[0].IdProperty.Should().Be(matchingProperty.IdProperty);
    }

    [Test]
    public async Task GetProperties_WithNoMatchingFilters_ShouldReturnEmptyListFromDatabase()
    {
        // Arrange
        var owner = CreateTestOwner();
        await CreateTestPropertyAsync(owner.IdOwner, "Test Property", "USA", "New York", 500000m, 2023);

        // Act
        var response = await Client.GetAsync("/api/v1/properties?country=NonExistentCountry");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var properties = await response.Content.ReadFromJsonAsync<List<GetPropertiesResponse>>();
        properties.Should().NotBeNull();
        properties!.Count.Should().Be(0);
    }

    [Test]
    public async Task GetProperties_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/v1/properties");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Helper Methods

    private async Task<Million.PropertiesService.Domain.Properties.Entities.Property> CreateTestPropertyAsync(
        Guid ownerId,
        string name = "Test Property",
        string country = "Test Country",
        string city = "Test City",
        decimal priceAmount = 500000m,
        int year = 2023)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
        
        var owner = await dbContext.Owners.FirstAsync(o => o.IdOwner == ownerId);
        var address = new Address("123 Test St", city, "12345", country);
        var price = new Money(priceAmount, "USD");
        
        var property = Million.PropertiesService.Domain.Properties.Entities.Property.Create(
            name,
            address,
            price,
            $"TEST-{Guid.NewGuid().ToString("N")[..8]}",
            year,
            owner
        );
        
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();
        
        return property;
    }

    #endregion
}
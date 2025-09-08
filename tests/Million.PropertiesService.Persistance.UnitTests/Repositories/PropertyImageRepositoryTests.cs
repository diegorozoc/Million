using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Persistance.Properties.Repositories;

namespace Million.PropertiesService.Persistance.UnitTests.Repositories;

[TestFixture]
public class PropertyImageRepositoryTests
{
    private PropertiesDbContext _context = null!;
    private PropertyImageRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PropertiesDbContext(options);
        _repository = new PropertyImageRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_ExistingImage_ShouldReturnImage()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image = PropertyImage.Create(property.IdProperty, "test.jpg", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddAsync(image);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(image.IdPropertyImage);

        // Assert
        result.Should().NotBeNull();
        result!.IdPropertyImage.Should().Be(image.IdPropertyImage);
        result.FileName.Should().Be("test.jpg");
    }

    [Test]
    public async Task GetByIdAsync_NonExistingImage_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetByPropertyIdAsync_MultipleImages_ShouldReturnImagesOrderedByCreatedAt()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image1 = PropertyImage.Create(property.IdProperty, "first.jpg", true);
        var image2 = PropertyImage.Create(property.IdProperty, "second.jpg", true);
        var image3 = PropertyImage.Create(property.IdProperty, "third.jpg", false);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddRangeAsync(image3, image1, image2); // Add out of order
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPropertyIdAsync(property.IdProperty);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(i => i.CreatedAt);
    }

    [Test]
    public async Task GetEnabledByPropertyIdAsync_MixedEnabledImages_ShouldReturnOnlyEnabled()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var enabledImage = PropertyImage.Create(property.IdProperty, "enabled.jpg", true);
        var disabledImage = PropertyImage.Create(property.IdProperty, "disabled.jpg", false);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddRangeAsync(enabledImage, disabledImage);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetEnabledByPropertyIdAsync(property.IdProperty);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(i => i.IdPropertyImage == enabledImage.IdPropertyImage);
        result.Should().NotContain(i => i.IdPropertyImage == disabledImage.IdPropertyImage);
    }

    [Test]
    public async Task GetByFileNameAsync_PartialMatch_ShouldReturnMatchingImages()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image1 = PropertyImage.Create(property.IdProperty, "house_front.jpg", true);
        var image2 = PropertyImage.Create(property.IdProperty, "house_back.jpg", true);
        var image3 = PropertyImage.Create(property.IdProperty, "garden.png", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddRangeAsync(image1, image2, image3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByFileNameAsync("house");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(i => i.IdPropertyImage == image1.IdPropertyImage);
        result.Should().Contain(i => i.IdPropertyImage == image2.IdPropertyImage);
        result.Should().NotContain(i => i.IdPropertyImage == image3.IdPropertyImage);
    }

    [Test]
    public async Task GetImagesByExtensionAsync_WithDot_ShouldReturnMatchingImages()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var jpgImage = PropertyImage.Create(property.IdProperty, "test.jpg", true);
        var pngImage = PropertyImage.Create(property.IdProperty, "test.png", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddRangeAsync(jpgImage, pngImage);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetImagesByExtensionAsync(".jpg");

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(i => i.IdPropertyImage == jpgImage.IdPropertyImage);
    }

    [Test]
    public async Task GetImagesByExtensionAsync_WithoutDot_ShouldReturnMatchingImages()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var jpgImage = PropertyImage.Create(property.IdProperty, "test.jpg", true);
        var pngImage = PropertyImage.Create(property.IdProperty, "test.png", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddRangeAsync(jpgImage, pngImage);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetImagesByExtensionAsync("png");

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(i => i.IdPropertyImage == pngImage.IdPropertyImage);
    }

    [Test]
    public async Task SaveAsync_NewImage_ShouldAddImage()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image = PropertyImage.Create(property.IdProperty, "new.jpg", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();

        // Act
        await _repository.SaveAsync(image);

        // Assert
        var savedImage = await _context.PropertyImages.FindAsync(image.IdPropertyImage);
        savedImage.Should().NotBeNull();
        savedImage!.FileName.Should().Be("new.jpg");
    }

    [Test]
    public async Task SaveAsync_ExistingImage_ShouldUpdateImage()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image = PropertyImage.Create(property.IdProperty, "original.jpg", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddAsync(image);
        await _context.SaveChangesAsync();

        image.UpdateFileName("updated.jpg");

        // Act
        await _repository.SaveAsync(image);

        // Assert
        var updatedImage = await _context.PropertyImages.FindAsync(image.IdPropertyImage);
        updatedImage.Should().NotBeNull();
        updatedImage!.FileName.Should().Be("updated.jpg");
    }

    [Test]
    public async Task DeleteAsync_ExistingImage_ShouldRemoveImage()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image = PropertyImage.Create(property.IdProperty, "delete.jpg", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddAsync(image);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(image.IdPropertyImage);

        // Assert
        var deletedImage = await _context.PropertyImages.FindAsync(image.IdPropertyImage);
        deletedImage.Should().BeNull();
    }

    [Test]
    public async Task ExistsAsync_ExistingImage_ShouldReturnTrue()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image = PropertyImage.Create(property.IdProperty, "exists.jpg", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddAsync(image);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(image.IdPropertyImage);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task PropertyHasImagesAsync_PropertyWithImages_ShouldReturnTrue()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image = PropertyImage.Create(property.IdProperty, "test.jpg", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddAsync(image);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PropertyHasImagesAsync(property.IdProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task GetImageCountByPropertyAsync_MultipleImages_ShouldReturnCorrectCount()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image1 = PropertyImage.Create(property.IdProperty, "image1.jpg", true);
        var image2 = PropertyImage.Create(property.IdProperty, "image2.jpg", true);
        var image3 = PropertyImage.Create(property.IdProperty, "image3.jpg", false);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddRangeAsync(image1, image2, image3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetImageCountByPropertyAsync(property.IdProperty);

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public async Task DeleteByPropertyIdAsync_MultipleImages_ShouldDeleteAllPropertyImages()
    {
        // Arrange
        var owner = CreateTestOwner();
        var property = CreateTestProperty("Test Property", owner);
        var image1 = PropertyImage.Create(property.IdProperty, "image1.jpg", true);
        var image2 = PropertyImage.Create(property.IdProperty, "image2.jpg", true);
        
        await _context.Owners.AddAsync(owner);
        await _context.Properties.AddAsync(property);
        await _context.PropertyImages.AddRangeAsync(image1, image2);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteByPropertyIdAsync(property.IdProperty);

        // Assert
        var remainingImages = await _context.PropertyImages
            .Where(pi => pi.IdProperty == property.IdProperty)
            .ToListAsync();
        remainingImages.Should().BeEmpty();
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
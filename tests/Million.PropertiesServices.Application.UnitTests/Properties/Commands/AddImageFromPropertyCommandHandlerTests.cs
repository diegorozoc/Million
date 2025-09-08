using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Million.PropertiesService.Application.Properties.Commands.AddImageFromProperty;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Moq;

namespace Million.PropertiesService.Application.UnitTests.Properties.Commands;

[TestFixture]
public class AddImageFromPropertyCommandHandlerTests
{
    private Mock<IPropertyRepository> _propertyRepositoryMock;
    private Mock<IPropertyImageRepository> _propertyImageRepositoryMock;
    private Mock<IValidator<AddImageFromPropertyCommand>> _validatorMock;
    private AddImageFromPropertyCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _propertyImageRepositoryMock = new Mock<IPropertyImageRepository>();
        _validatorMock = new Mock<IValidator<AddImageFromPropertyCommand>>();
        
        _handler = new AddImageFromPropertyCommandHandler(
            _propertyRepositoryMock.Object,
            _propertyImageRepositoryMock.Object,
            _validatorMock.Object);
    }

    [Test]
    public async Task Handle_ValidRequest_ShouldAddImageAndReturnResponse()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var fileName = "test-image.jpg";
        var enabled = true;
        
        var request = new AddImageFromPropertyCommand(propertyId, fileName, enabled);
        
        var ownerAddress = new Address("123 Owner St", "Owner City", "12345", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-30));
        var owner = Owner.Create("Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("456 Property St", "Property City", "54321", "USA");
        var property = Property.Create("Test Property", propertyAddress, new Money(200000, "USD"), "PROP001", 2023, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdPropertyImage.Should().NotBeEmpty();
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyImageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<PropertyImage>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequestDisabledImage_ShouldAddDisabledImageAndReturnResponse()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var fileName = "disabled-image.png";
        var enabled = false;
        
        var request = new AddImageFromPropertyCommand(propertyId, fileName, enabled);
        
        var ownerAddress = new Address("123 Owner St", "Owner City", "12345", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-25));
        var owner = Owner.Create("Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("789 Property Ave", "Property City", "98765", "USA");
        var property = Property.Create("Another Property", propertyAddress, new Money(300000, "USD"), "PROP002", 2022, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IdPropertyImage.Should().NotBeEmpty();
        
        _propertyImageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<PropertyImage>()), Times.Once);
    }

    [Test]
    public void Handle_InvalidValidation_ShouldThrowValidationException()
    {
        // Arrange
        var request = new AddImageFromPropertyCommand(Guid.Empty, "", true);
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("IdProperty", "Property ID is required"),
            new ValidationFailure("FileName", "File name is required")
        });

        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(request, CancellationToken.None));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Property ID is required"));
        Assert.That(exception.Message, Does.Contain("File name is required"));
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _propertyImageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<PropertyImage>()), Times.Never);
    }

    [Test]
    public void Handle_PropertyNotFound_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new AddImageFromPropertyCommand(propertyId, "image.jpg", true);
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
        _propertyImageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<PropertyImage>()), Times.Never);
    }

    [Test]
    public void Handle_EmptyFileName_ShouldThrowValidationException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var request = new AddImageFromPropertyCommand(propertyId, "", true);
        
        var ownerAddress = new Address("123 Owner St", "Owner City", "12345", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-35));
        var owner = Owner.Create("Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("456 Property St", "Property City", "54321", "USA");
        var property = Property.Create("Test Property", propertyAddress, new Money(200000, "USD"), "PROP001", 2023, owner);

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
        Assert.That(exception.Message, Does.Contain("File name cannot be empty"));
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
        _propertyImageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<PropertyImage>()), Times.Never);
    }

    [Test]
    public async Task Handle_ValidRequestMultipleImages_ShouldAddEachImageSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var firstFileName = "image1.jpg";
        var secondFileName = "image2.png";
        
        var ownerAddress = new Address("456 Owner Ave", "Owner City", "67890", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-32));
        var owner = Owner.Create("Multi Image Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("111 Multi Property St", "Property City", "33333", "USA");
        var property = Property.Create("Multi Image Property", propertyAddress, new Money(400000, "USD"), "PROP005", 2020, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<AddImageFromPropertyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        var firstRequest = new AddImageFromPropertyCommand(propertyId, firstFileName, true);
        var secondRequest = new AddImageFromPropertyCommand(propertyId, secondFileName, false);

        // Act
        var firstResult = await _handler.Handle(firstRequest, CancellationToken.None);
        var secondResult = await _handler.Handle(secondRequest, CancellationToken.None);

        // Assert
        firstResult.Should().NotBeNull();
        firstResult.IdPropertyImage.Should().NotBeEmpty();
        
        secondResult.Should().NotBeNull();
        secondResult.IdPropertyImage.Should().NotBeEmpty();
        
        firstResult.IdPropertyImage.Should().NotBe(secondResult.IdPropertyImage);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Exactly(2));
        _propertyImageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<PropertyImage>()), Times.Exactly(2));
    }

    [Test]
    public async Task Handle_DifferentFileExtensions_ShouldAddImagesSuccessfully()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var jpgFileName = "photo.jpg";
        var pngFileName = "diagram.png";
        var gifFileName = "animation.gif";
        
        var ownerAddress = new Address("789 Owner Blvd", "Owner City", "11111", "USA");
        var ownerBirthDate = new DateOfBirth(DateTime.Today.AddYears(-28));
        var owner = Owner.Create("Extension Test Owner", ownerAddress, ownerBirthDate);
        var propertyAddress = new Address("222 Extension Property Ave", "Property City", "44444", "USA");
        var property = Property.Create("Extension Test Property", propertyAddress, new Money(350000, "USD"), "PROP006", 2019, owner);

        var validationResult = new ValidationResult();

        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<AddImageFromPropertyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        var jpgRequest = new AddImageFromPropertyCommand(propertyId, jpgFileName, true);
        var pngRequest = new AddImageFromPropertyCommand(propertyId, pngFileName, true);
        var gifRequest = new AddImageFromPropertyCommand(propertyId, gifFileName, false);

        // Act
        var jpgResult = await _handler.Handle(jpgRequest, CancellationToken.None);
        var pngResult = await _handler.Handle(pngRequest, CancellationToken.None);
        var gifResult = await _handler.Handle(gifRequest, CancellationToken.None);

        // Assert
        jpgResult.Should().NotBeNull();
        jpgResult.IdPropertyImage.Should().NotBeEmpty();
        
        pngResult.Should().NotBeNull();
        pngResult.IdPropertyImage.Should().NotBeEmpty();
        
        gifResult.Should().NotBeNull();
        gifResult.IdPropertyImage.Should().NotBeEmpty();
        
        // All image IDs should be unique
        jpgResult.IdPropertyImage.Should().NotBe(pngResult.IdPropertyImage);
        jpgResult.IdPropertyImage.Should().NotBe(gifResult.IdPropertyImage);
        pngResult.IdPropertyImage.Should().NotBe(gifResult.IdPropertyImage);
        
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Exactly(3));
        _propertyImageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<PropertyImage>()), Times.Exactly(3));
    }
}
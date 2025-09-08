using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Million.PropertiesService.Api.Controllers;
using Million.PropertiesService.Api.Models;
using Million.PropertiesService.Application.Properties.Commands.AddImageFromProperty;
using Million.PropertiesService.Application.Properties.Commands.ChangePrice;
using Million.PropertiesService.Application.Properties.Commands.UpdateProperty;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesService.Application.Properties.Queries.GetProperties;
using Million.PropertiesServices.Application.Properties.Commands.CreatePropertyBuilding;
using Million.PropertiesServices.Application.Properties.Models;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Moq;

namespace Million.PropertiesService.Api.UnitTests.Controllers;

[TestFixture]
public sealed class PropertiesControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private Mock<IMapper> _mapperMock;
    private PropertiesController _controller;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _controller = new PropertiesController(_mapperMock.Object);

        // Set up HttpContext and Services for the controller
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ISender>(_mediatorMock.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [TearDown]
    public void TearDown()
    {
        // Controller doesn't implement IDisposable
    }

    #region CreatePropertyBuilding Tests

    [Test]
    public async Task CreatePropertyBuilding_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var newProperty = new NewProperty(
            "Test Property",
            new Address("123 Test St", "Test City", "12345", "Test Country"),
            new Money(500000m, "USD"),
            "TEST-001",
            2023,
            Guid.NewGuid()
        );

        var command = new CreatePropertyBuildingCommand(
            newProperty.Name,
            newProperty.Address,
            newProperty.Price,
            newProperty.CodeInternal,
            newProperty.Year,
            newProperty.IdOwner
        );

        var response = new CreatePropertyBuildingResponse
        {
            IdProperty = Guid.NewGuid()
        };

        _mapperMock.Setup(m => m.Map<CreatePropertyBuildingCommand>(newProperty))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePropertyBuildingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreatePropertyBuilding(newProperty, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedResult>();
        var createdResult = result.Result as CreatedResult;
        createdResult!.Value.Should().Be(response);
        createdResult.Location.Should().Be($"/api/v1.0/properties/{response.IdProperty}");
    }

    [Test]
    public async Task CreatePropertyBuilding_WithMediatorException_ThrowsException()
    {
        // Arrange
        var newProperty = new NewProperty(
            "Test Property",
            new Address("123 Test St", "Test City", "12345", "Test Country"),
            new Money(500000m, "USD"),
            "TEST-001",
            2023,
            Guid.NewGuid()
        );

        var command = new CreatePropertyBuildingCommand(
            newProperty.Name,
            newProperty.Address,
            newProperty.Price,
            newProperty.CodeInternal,
            newProperty.Year,
            newProperty.IdOwner
        );

        _mapperMock.Setup(m => m.Map<CreatePropertyBuildingCommand>(newProperty))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePropertyBuildingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Property creation failed"));

        // Act & Assert
        await _controller.Invoking(c => c.CreatePropertyBuilding(newProperty, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Property creation failed");
    }

    #endregion

    #region AddPropertyImage Tests

    [Test]
    public async Task AddPropertyImage_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newPropertyImage = new NewPropertyImage("test-image.jpg", true, propertyId);

        var command = new AddImageFromPropertyCommand(propertyId, "test-image.jpg", true);
        var response = new AddImageFromPropertyResponse
        {
            IdPropertyImage = Guid.NewGuid()
        };

        _mapperMock.Setup(m => m.Map<AddImageFromPropertyCommand>(newPropertyImage))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(It.IsAny<AddImageFromPropertyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.AddPropertyImage(newPropertyImage, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedResult>();
        var createdResult = result.Result as CreatedResult;
        createdResult!.Value.Should().Be(response);
        createdResult.Location.Should().Be($"/api/v1.0/properties/images/{response.IdPropertyImage}");
    }

    [Test]
    public async Task AddPropertyImage_WithMediatorException_ThrowsException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newPropertyImage = new NewPropertyImage("test-image.jpg", true, propertyId);
        var command = new AddImageFromPropertyCommand(propertyId, "test-image.jpg", true);

        _mapperMock.Setup(m => m.Map<AddImageFromPropertyCommand>(newPropertyImage))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(It.IsAny<AddImageFromPropertyCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Property not found"));

        // Act & Assert
        await _controller.Invoking(c => c.AddPropertyImage(newPropertyImage, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Property not found");
    }

    #endregion

    #region ChangePropertyPrice Tests

    [Test]
    public async Task ChangePropertyPrice_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newPrice = new Money(600000m, "USD");
        var changePriceRequest = new ChangePrice(newPrice);

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePriceCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ChangePropertyPrice(propertyId, changePriceRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<ChangePriceCommand>(c => c.Price == newPrice && c.IdProperty == propertyId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ChangePropertyPrice_WithMediatorException_ThrowsException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newPrice = new Money(600000m, "USD");
        var changePriceRequest = new ChangePrice(newPrice);

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePriceCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Property not found"));

        // Act & Assert
        await _controller.Invoking(c => c.ChangePropertyPrice(propertyId, changePriceRequest, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Property not found");
    }

    #endregion

    #region UpdateProperty Tests

    [Test]
    public async Task UpdateProperty_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var updateRequest = new UpdatePropertyRequest(
            "Updated Property Name",
            new Address("456 Updated St", "Updated City", "54321", "Updated Country"),
            2024,
            ownerId
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePropertyCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateProperty(propertyId, updateRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdatePropertyCommand>(c => 
                c.Name == updateRequest.Name &&
                c.Address == updateRequest.Address &&
                c.Year == updateRequest.Year &&
                c.IdOwner == updateRequest.IdOwner &&
                c.IdProperty == propertyId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateProperty_WithMediatorException_ThrowsException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var updateRequest = new UpdatePropertyRequest(
            "Updated Property Name",
            new Address("456 Updated St", "Updated City", "54321", "Updated Country"),
            2024,
            ownerId
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePropertyCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Property not found"));

        // Act & Assert
        await _controller.Invoking(c => c.UpdateProperty(propertyId, updateRequest, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Property not found");
    }

    #endregion

    #region GetProperties Tests

    [Test]
    public async Task GetProperties_WithNoFilters_ReturnsOkWithProperties()
    {
        // Arrange
        var properties = new List<GetPropertiesResponse>
        {
            new GetPropertiesResponse(
                Guid.NewGuid(),
                "Property 1",
                new Address("123 Test St", "Test City", "12345", "Test Country"),
                new Money(500000m, "USD"),
                2023,
                new OwnerResponse(Guid.NewGuid(), "Owner 1")
            ),
            new GetPropertiesResponse(
                Guid.NewGuid(),
                "Property 2",
                new Address("456 Test Ave", "Test City", "12346", "Test Country"),
                new Money(750000m, "USD"),
                2024,
                new OwnerResponse(Guid.NewGuid(), "Owner 2")
            )
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPropertiesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(properties);

        // Act
        var result = await _controller.GetProperties(null, null, null, null, null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(properties);
    }

    [Test]
    public async Task GetProperties_WithFilters_ReturnsOkWithFilteredProperties()
    {
        // Arrange
        var country = "Test Country";
        var city = "Test City";
        var minPrice = 400000m;
        var maxPrice = 600000m;
        var year = 2023;

        var properties = new List<GetPropertiesResponse>
        {
            new GetPropertiesResponse(
                Guid.NewGuid(),
                "Property 1",
                new Address("123 Test St", city, "12345", country),
                new Money(500000m, "USD"),
                year,
                new OwnerResponse(Guid.NewGuid(), "Owner 1")
            )
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPropertiesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(properties);

        // Act
        var result = await _controller.GetProperties(country, city, minPrice, maxPrice, year, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(properties);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetPropertiesQuery>(q => 
                q.Country == country &&
                q.City == city &&
                q.minPrice == minPrice &&
                q.maxPrice == maxPrice &&
                q.Year == year),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetProperties_WithMediatorException_ThrowsException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPropertiesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        await _controller.Invoking(c => c.GetProperties(null, null, null, null, null, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    [Test]
    public async Task GetProperties_ReturnsEmptyList_WhenNoPropertiesFound()
    {
        // Arrange
        var emptyProperties = new List<GetPropertiesResponse>();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPropertiesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyProperties);

        // Act
        var result = await _controller.GetProperties(null, null, null, null, null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(emptyProperties);
    }

    #endregion

    #region Mapper Verification Tests

    [Test]
    public async Task CreatePropertyBuilding_VerifyMapperCalled()
    {
        // Arrange
        var newProperty = new NewProperty(
            "Test Property",
            new Address("123 Test St", "Test City", "12345", "Test Country"),
            new Money(500000m, "USD"),
            "TEST-001",
            2023,
            Guid.NewGuid()
        );

        var command = new CreatePropertyBuildingCommand(
            newProperty.Name,
            newProperty.Address,
            newProperty.Price,
            newProperty.CodeInternal,
            newProperty.Year,
            newProperty.IdOwner
        );

        var response = new CreatePropertyBuildingResponse
        {
            IdProperty = Guid.NewGuid()
        };

        _mapperMock.Setup(m => m.Map<CreatePropertyBuildingCommand>(newProperty))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _controller.CreatePropertyBuilding(newProperty, CancellationToken.None);

        // Assert
        _mapperMock.Verify(m => m.Map<CreatePropertyBuildingCommand>(newProperty), Times.Once);
    }

    [Test]
    public async Task AddPropertyImage_VerifyMapperCalled()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var newPropertyImage = new NewPropertyImage("test-image.jpg", true, propertyId);
        var command = new AddImageFromPropertyCommand(propertyId, "test-image.jpg", true);
        var response = new AddImageFromPropertyResponse
        {
            IdPropertyImage = Guid.NewGuid()
        };

        _mapperMock.Setup(m => m.Map<AddImageFromPropertyCommand>(newPropertyImage))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _controller.AddPropertyImage(newPropertyImage, CancellationToken.None);

        // Assert
        _mapperMock.Verify(m => m.Map<AddImageFromPropertyCommand>(newPropertyImage), Times.Once);
    }

    #endregion
}

using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Million.PropertiesService.Api.Models;
using Million.PropertiesService.Api.Web;
using Million.PropertiesService.Application.Properties.Commands.AddImageFromProperty;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesServices.Application.Properties.Commands.CreatePropertyBuilding;
using Million.PropertiesService.Application.Properties.Commands.ChangePrice;
using Million.PropertiesService.Application.Properties.Commands.UpdateProperty;
using Million.PropertiesService.Application.Properties.Queries.GetProperties;
using Million.PropertiesServices.Application.Properties.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Million.PropertiesService.Api.Controllers;

/// <summary>
/// Properties management endpoints for creating, updating, and managing real estate properties
/// </summary>
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[SwaggerTag("Manage properties including creation, updates, and property information")]
public class PropertiesController : ApiControllerBase
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the PropertiesController
    /// </summary>
    /// <param name="mapper">AutoMapper instance for object mapping</param>
    public PropertiesController(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new property in the system
    /// </summary>
    /// <param name="newProperty">The property details to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created property information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1.0/properties
    ///     {
    ///         "name": "Luxury Downtown Apartment",
    ///         "address": {
    ///             "street": "123 Main Street",
    ///             "city": "New York",
    ///             "postalCode": "10001",
    ///             "country": "USA"
    ///         },
    ///         "price": {
    ///             "amount": 500000.00,
    ///             "currency": "USD"
    ///         },
    ///         "codeInternal": "PROP-001",
    ///         "year": 2023,
    ///         "idOwner": "12345678-1234-1234-1234-123456789012"
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">Property created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="409">Conflict - property with same internal code already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [SwaggerOperation(
        Summary = "Create a new property",
        Description = "Creates a new property with the provided details including address, price, and owner information",
        OperationId = "CreateProperty",
        Tags = new[] { "Properties" }
    )]
    [SwaggerResponse((int)HttpStatusCode.Created, "Property created successfully", typeof(CreatePropertyBuildingResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request data")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Authentication required")]
    [SwaggerResponse((int)HttpStatusCode.Conflict, "Property with same internal code already exists")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error")]
    [ProducesResponseType(typeof(CreatePropertyBuildingResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<CreatePropertyBuildingResponse>> CreatePropertyBuilding(
        [FromBody][SwaggerParameter("Property details", Required = true)] NewProperty newProperty,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreatePropertyBuildingCommand>(newProperty);
        var result = await Mediator.Send(command, cancellationToken);
        return Created($"/api/v1.0/properties/{result.IdProperty}", result);
    }

    /// <summary>
    /// Adds a new image to an existing property
    /// </summary>
    /// <param name="newPropertyImage">The property image details to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created property image information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1.0/properties/images
    ///     {
    ///         "idProperty": "12345678-1234-1234-1234-123456789012",
    ///         "fileName": "property-photo-01.jpg",
    ///         "enabled": true
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">Property image created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Property not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("images")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [SwaggerOperation(
        Summary = "Add a new image to a property",
        Description = "Creates a new property image with the provided details including fileName, enabled status and property information",
        OperationId = "AddPropertyImage",
        Tags = new[] { "Properties" }
    )]
    [SwaggerResponse((int)HttpStatusCode.Created, "Property image created successfully", typeof(AddImageFromPropertyResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request data")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Authentication required")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Property not found")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error")]
    [ProducesResponseType(typeof(AddImageFromPropertyResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<AddImageFromPropertyResponse>> AddPropertyImage(
        [FromBody][SwaggerParameter("Property image details", Required = true)] NewPropertyImage newPropertyImage,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<AddImageFromPropertyCommand>(newPropertyImage);
        var result = await Mediator.Send(command, cancellationToken);
        return Created($"/api/v1.0/properties/images/{result.IdPropertyImage}", result);
    }

    /// <summary>
    /// Changes the price of an existing property
    /// </summary>
    /// <param name="idProperty">The unique identifier of the property</param>
    /// <param name="changePriceRequest">The new price details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/v1.0/properties/{propertyId}/price
    ///     {
    ///         "newPrice": {
    ///             "amount": 600000.00,
    ///             "currency": "USD"
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">Price updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Property not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{idProperty:guid}/price")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        Summary = "Change property price",
        Description = "Updates the price of an existing property with the provided monetary value",
        OperationId = "ChangePropertyPrice",
        Tags = new[] { "Properties" }
    )]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Price updated successfully")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request data")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Authentication required")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Property not found")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ChangePropertyPrice(
        [FromRoute][SwaggerParameter("Property unique identifier", Required = true)] Guid idProperty,
        [FromBody][SwaggerParameter("New price details", Required = true)] ChangePrice changePriceRequest,
        CancellationToken cancellationToken)
    {
        var command = new ChangePriceCommand(changePriceRequest.NewPrice, idProperty);
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates an existing property with new information
    /// </summary>
    /// <param name="idProperty">The unique identifier of the property</param>
    /// <param name="updatePropertyRequest">The property details to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/v1.0/properties/{propertyId}
    ///     {
    ///         "name": "Updated Luxury Downtown Apartment",
    ///         "address": {
    ///             "street": "456 New Street",
    ///             "city": "New York",
    ///             "postalCode": "10002",
    ///             "country": "USA"
    ///         },
    ///         "year": 2024,
    ///         "idOwner": "87654321-4321-4321-4321-210987654321"
    ///     }
    /// 
    /// All fields are optional - only provide the fields you want to update.
    /// </remarks>
    /// <response code="204">Property updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Property not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{idProperty:guid}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [SwaggerOperation(
        Summary = "Update an existing property",
        Description = "Updates an existing property with the provided details. All fields are optional - only provided fields will be updated.",
        OperationId = "UpdateProperty",
        Tags = new[] { "Properties" }
    )]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Property updated successfully")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request data")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Authentication required")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Property not found")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> UpdateProperty(
        [FromRoute][SwaggerParameter("Property unique identifier", Required = true)] Guid idProperty,
        [FromBody][SwaggerParameter("Property update details", Required = true)] UpdatePropertyRequest updatePropertyRequest,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePropertyCommand(
            updatePropertyRequest.Name,
            updatePropertyRequest.Address,
            updatePropertyRequest.Year,
            updatePropertyRequest.IdOwner,
            idProperty);
        
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets properties by filters
    /// </summary>
    /// <param name="country">Filter by country (optional, case-insensitive)</param>
    /// <param name="city">Filter by city (optional, case-insensitive)</param>
    /// <param name="minPrice">Minimum price filter (optional)</param>
    /// <param name="maxPrice">Maximum price filter (optional)</param>
    /// <param name="year">Filter by year built (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of properties matching the filters</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/v1.0/properties?country=USA&amp;city=Miami&amp;minPrice=200000&amp;maxPrice=800000&amp;year=2020
    /// 
    /// All query parameters are optional. You can use any combination of filters.
    /// </remarks>
    /// <response code="200">Properties retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get properties with filters",
        Description = "Retrieves properties based on optional filters for country, city, price range, and year built",
        OperationId = "GetProperties",
        Tags = new[] { "Properties" }
    )]
    [SwaggerResponse((int)HttpStatusCode.OK, "Properties retrieved successfully", typeof(IReadOnlyList<GetPropertiesResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Authentication required")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error")]
    [ProducesResponseType(typeof(IReadOnlyList<GetPropertiesResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<GetPropertiesResponse>>> GetProperties(
        [FromQuery][SwaggerParameter("Filter by country (case-insensitive)")] string? country,
        [FromQuery][SwaggerParameter("Filter by city (case-insensitive)")] string? city,
        [FromQuery][SwaggerParameter("Minimum price filter")] decimal? minPrice,
        [FromQuery][SwaggerParameter("Maximum price filter")] decimal? maxPrice,
        [FromQuery][SwaggerParameter("Filter by year built")] int? year,
        CancellationToken cancellationToken)
    {
        var query = new GetPropertiesQuery(country, city, minPrice, maxPrice, year);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
using Million.PropertiesService.Domain.Common.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Million.PropertiesService.Api.Models;

/// <summary>
/// Represents a request to update an existing property
/// </summary>
/// <param name="Name">Optional new name for the property</param>
/// <param name="Address">Optional new address for the property</param>
/// <param name="Year">Optional new year the property was built</param>
/// <param name="IdOwner">Optional new owner identifier</param>
[SwaggerSchema(
    Title = "Update Property Request",
    Description = "Data transfer object for updating an existing property. All fields are optional - only provided fields will be updated."
)]
public record UpdatePropertyRequest(
    [SwaggerSchema("The new display name of the property")]
    [StringLength(200, MinimumLength = 1)]
    string? Name,
    
    [SwaggerSchema("New complete address information including street, city, postal code, and country")]
    Address? Address,
    
    [SwaggerSchema("The new year the property was built")]
    [Range(1800, 2100)]
    int? Year,
    
    [SwaggerSchema("The new unique identifier of the property owner")]
    Guid? IdOwner
);
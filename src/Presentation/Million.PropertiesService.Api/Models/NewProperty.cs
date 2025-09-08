using Million.PropertiesService.Domain.Common.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Million.PropertiesService.Api.Models;

/// <summary>
/// Represents a new property to be created in the system
/// </summary>
/// <param name="Name">The display name of the property</param>
/// <param name="Address">The complete address information of the property</param>
/// <param name="Price">The monetary value of the property</param>
/// <param name="CodeInternal">The internal unique identifier code for the property</param>
/// <param name="Year">The year the property was built</param>
/// <param name="IdOwner">The unique identifier of the property owner</param>
[SwaggerSchema(
    Title = "New Property",
    Description = "Data transfer object for creating a new property in the system"
)]
public record NewProperty(
    [SwaggerSchema("The display name of the property")]
    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Name,
    
    [SwaggerSchema("Complete address information including street, city, postal code, and country")]
    [Required]
    Address Address,
    
    [SwaggerSchema("Monetary value of the property including amount and currency")]
    [Required]
    Money Price,
    
    [SwaggerSchema("Internal unique identifier code for the property")]
    [Required]
    [StringLength(50, MinimumLength = 1)]
    string CodeInternal,
    
    [SwaggerSchema("The year the property was built")]
    [Range(1800, 2100)]
    int Year,
    
    [SwaggerSchema("The unique identifier of the property owner")]
    [Required]
    Guid IdOwner
);
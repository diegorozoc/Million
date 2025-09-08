using Million.PropertiesService.Domain.Common.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Million.PropertiesService.Api.Models;

/// <summary>
/// Represents a request to change the price of an existing property
/// </summary>
/// <param name="NewPrice">The new monetary value for the property</param>
[SwaggerSchema(
    Title = "Change Price Request",
    Description = "Data transfer object for changing the price of an existing property"
)]
public record ChangePrice(
    [SwaggerSchema("New monetary value for the property including amount and currency")]
    [Required]
    Money NewPrice
);
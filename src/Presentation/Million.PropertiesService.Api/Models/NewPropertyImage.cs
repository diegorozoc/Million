using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Million.PropertiesService.Api.Models;

/// <summary>
/// Represents a new property iamge to be created in the system
/// </summary>
/// <param name="FileName">The file name of the property image</param>
/// <param name="Enable">Enable/Disable status of the image</param>
/// <param name="IdProperty">The unique identifier of the property </param>
[SwaggerSchema(
    Title = "New Property Image",
    Description = "Data transfer object for creating a new property image in the system"
)]
public record NewPropertyImage(
    [SwaggerSchema("The display file name of the property")]
    [Required]
    [StringLength(255, MinimumLength = 1)]
    string FileName,

    [SwaggerSchema("Complete address information including street, city, postal code, and country")]
    [Required]
    bool Enable,

    [SwaggerSchema("The unique identifier of the property")]
    [Required]
    Guid IdProperty
);
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.Application.Properties.Models;

public record GetPropertiesResponse(
    Guid IdProperty,
    string Name,
    Address Address,
    Money Price,
    int Year,
    OwnerResponse Owner);

public record OwnerResponse(
    Guid IdOwner,
    string Name);

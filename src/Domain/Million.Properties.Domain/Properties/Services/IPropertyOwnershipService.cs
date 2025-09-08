using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Owners.Entities;

namespace Million.PropertiesService.Domain.Properties.Services;

/// <summary>
/// Domain service for managing property ownership business rules
/// </summary>
public interface IPropertyOwnershipService
{
    /// <summary>
    /// Validates if an owner can acquire a new property based on business rules
    /// </summary>
    Task<PropertyOwnershipValidationResult> ValidateOwnerCanAcquirePropertyAsync(Owner owner, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Assigns property ownership to an owner with all necessary business rule validation
    /// </summary>
    Task AssignPropertyToOwnerAsync(Property property, Owner owner, CancellationToken cancellationToken = default);
}

public class PropertyOwnershipValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    
    public static PropertyOwnershipValidationResult Success() => new() { IsValid = true };
    public static PropertyOwnershipValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
}
namespace Million.PropertiesService.Domain.Properties.Services;

/// <summary>
/// Domain service for property business validation rules
/// </summary>
public interface IPropertyValidationService
{
    /// <summary>
    /// Validates if a property code internal is unique across the system
    /// </summary>
    Task<PropertyValidationResult> ValidateCodeInternalUniquenessAsync(string codeInternal, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates all business rules for property creation
    /// </summary>
    Task<PropertyValidationResult> ValidatePropertyForCreationAsync(string name, string codeInternal, int year, CancellationToken cancellationToken = default);
}

public class PropertyValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    
    public static PropertyValidationResult Success() => new() { IsValid = true };
    public static PropertyValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
}
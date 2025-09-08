using Million.PropertiesService.Domain.Properties.Repositories;

namespace Million.PropertiesService.Domain.Properties.Services;

/// <summary>
/// Domain service implementing property validation business rules
/// </summary>
public class PropertyValidationService : IPropertyValidationService
{
    private readonly IPropertyRepository _propertyRepository;

    public PropertyValidationService(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyValidationResult> ValidateCodeInternalUniquenessAsync(string codeInternal, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codeInternal))
        {
            return PropertyValidationResult.Failure("Property code internal cannot be empty.");
        }

        // Business rule: Property code internal must be unique
        var exists = await _propertyRepository.CodeInternalExistsAsync(codeInternal);
        if (exists)
        {
            return PropertyValidationResult.Failure($"Property with code internal '{codeInternal}' already exists.");
        }

        return PropertyValidationResult.Success();
    }

    public async Task<PropertyValidationResult> ValidatePropertyForCreationAsync(string name, string codeInternal, int year, CancellationToken cancellationToken = default)
    {
        // Business rule: Property name cannot be empty
        if (string.IsNullOrWhiteSpace(name))
        {
            return PropertyValidationResult.Failure("Property name cannot be empty.");
        }

        // Business rule: Year must be valid
        if (year < 1800 || year > DateTime.Now.Year)
        {
            return PropertyValidationResult.Failure($"Property year must be between 1800 and {DateTime.Now.Year}.");
        }

        // Business rule: Code internal must be unique
        var codeValidation = await ValidateCodeInternalUniquenessAsync(codeInternal, cancellationToken);
        if (!codeValidation.IsValid)
        {
            return codeValidation;
        }

        return PropertyValidationResult.Success();
    }
}
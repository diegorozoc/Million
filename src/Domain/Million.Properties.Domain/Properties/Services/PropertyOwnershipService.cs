using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Owners.Entities;

namespace Million.PropertiesService.Domain.Properties.Services;

/// <summary>
/// Domain service implementing property ownership business rules
/// </summary>
public class PropertyOwnershipService : IPropertyOwnershipService
{
    private readonly IPropertyRepository _propertyRepository;

    public PropertyOwnershipService(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public Task<PropertyOwnershipValidationResult> ValidateOwnerCanAcquirePropertyAsync(Owner owner, CancellationToken cancellationToken = default)
    {
        // Business rule: Owner must be an adult
        if (!owner.IsAdult())
        {
            return Task.FromResult(PropertyOwnershipValidationResult.Failure($"Owner '{owner.Name}' must be at least 18 years old to own property."));
        }

        // Business rule: Owner cannot exceed maximum property limit
        if (!owner.CanOwnMoreProperties())
        {
            return Task.FromResult(PropertyOwnershipValidationResult.Failure($"Owner '{owner.Name}' has reached the maximum number of properties they can own."));
        }

        return Task.FromResult(PropertyOwnershipValidationResult.Success());
    }

    public async Task AssignPropertyToOwnerAsync(Property property, Owner owner, CancellationToken cancellationToken = default)
    {
        // Validate ownership rules
        var validationResult = await ValidateOwnerCanAcquirePropertyAsync(owner, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        // Assign property to owner (domain logic)
        property.SetOwner(owner);
        
        // Update owner's property collection
        owner.AddProperty(property.IdProperty);
    }
}
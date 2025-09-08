using FluentValidation;
using MediatR;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesServices.Application.Properties.Models;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Properties.Services;
using Million.PropertiesService.Domain.Owners.Repositories;

namespace Million.PropertiesServices.Application.Properties.Commands.CreatePropertyBuilding;

public sealed class CreatePropertyBuildingCommandHandler(
    IPropertyRepository propertyRepository,
    IOwnerRepository ownerRepository,
    IValidator<CreatePropertyBuildingCommand> validator,
    IDomainEventDispatcher domainEventDispatcher,
    IPropertyValidationService propertyValidationService,
    IPropertyOwnershipService propertyOwnershipService) 
    : IRequestHandler<CreatePropertyBuildingCommand, CreatePropertyBuildingResponse>
{
    public async Task<CreatePropertyBuildingResponse> Handle(CreatePropertyBuildingCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }

        // Use domain service to validate property business rules
        var propertyValidation = await propertyValidationService.ValidatePropertyForCreationAsync(
            request.Name, request.CodeInternal, request.Year, cancellationToken);
        if (!propertyValidation.IsValid)
        {
            throw new InvalidOperationException(propertyValidation.ErrorMessage);
        }

        // Verify that the owner exists
        var owner = await ownerRepository.GetByIdAsync(request.IdOwner);
        if (owner == null)
        {
            throw new InvalidOperationException($"Owner with ID '{request.IdOwner}' does not exist.");
        }

        // Use domain service to validate ownership rules
        var ownershipValidation = await propertyOwnershipService.ValidateOwnerCanAcquirePropertyAsync(owner, cancellationToken);
        if (!ownershipValidation.IsValid)
        {
            throw new InvalidOperationException(ownershipValidation.ErrorMessage);
        }

        try
        {
            // Create the property using the domain factory method
            var property = Property.Create(
                request.Name,
                request.Address,
                request.Price,
                request.CodeInternal,
                request.Year,
                owner
            );

            // Use domain service to assign property to owner with all business rules
            await propertyOwnershipService.AssignPropertyToOwnerAsync(property, owner, cancellationToken);

            // Save the property
            await propertyRepository.SaveAsync(property);

            // Dispatch domain events
            await domainEventDispatcher.DispatchAsync(property.DomainEvents, cancellationToken);
            
            // Clear domain events after publishing
            property.ClearDomainEvents();

            // Return the response with the created property ID
            return new CreatePropertyBuildingResponse
            {
                IdProperty = property.IdProperty
            };
        }
        catch (ArgumentException ex)
        {
            // Re-throw domain validation errors as validation exceptions
            throw new ValidationException($"Domain validation failed: {ex.Message}");
        }
    }
}
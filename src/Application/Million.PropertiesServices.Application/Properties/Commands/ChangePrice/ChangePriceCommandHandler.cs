using FluentValidation;
using MediatR;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain.Properties.Repositories;

namespace Million.PropertiesService.Application.Properties.Commands.ChangePrice;


public sealed class ChangePriceCommandHandler(
    IPropertyRepository propertyRepository, 
    IValidator<ChangePriceCommand> validator,
    IDomainEventDispatcher domainEventDispatcher) : IRequestHandler<ChangePriceCommand>
{
    public async Task Handle(ChangePriceCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }

        try
        {
            // Retrieve the existing property by ID
            var property = await propertyRepository.GetByIdAsync(request.IdProperty);
            if (property == null)
            {
                throw new ValidationException($"Property with ID {request.IdProperty} not found");
            }

            // Use the ChangePrice method from the Property domain entity
            property.ChangePrice(request.Price);

            // Save the updated property
            await propertyRepository.SaveAsync(property);

            // Dispatch domain events
            await domainEventDispatcher.DispatchAsync(property.DomainEvents, cancellationToken);
            
            // Clear domain events after publishing
            property.ClearDomainEvents();
        }
        catch (ArgumentException ex)
        {
            // Re-throw domain validation errors as validation exceptions
            throw new ValidationException($"Domain validation failed: {ex.Message}");
        }
    }
}

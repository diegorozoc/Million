using FluentValidation;
using MediatR;
using Million.PropertiesService.Domain.Owners.Repositories;
using Million.PropertiesService.Domain.Properties.Repositories;

namespace Million.PropertiesService.Application.Properties.Commands.UpdateProperty;

public sealed class UpdatePropertyCommandHandler(
    IPropertyRepository propertyRepository,
    IOwnerRepository ownerRepository,
    IValidator<UpdatePropertyCommand> validator)
    : IRequestHandler<UpdatePropertyCommand>
{
    public async Task Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
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
            if (property is null)
            {
                throw new ValidationException($"Property with ID {request.IdProperty} not found");
            }

            // Verify that the owner exists
            if(request.IdOwner is not null)
            {
                var owner = await ownerRepository.GetByIdAsync(request.IdOwner.Value);
                if (owner is null)
                {
                    throw new InvalidOperationException($"Owner with ID '{request.IdOwner}' does not exist.");
                }
                property.SetOwner(owner);
            }

            if (request.Address is not null)
                property.UpdateAddress(request.Address);

            if(request.Name is not null)
                property.UpdateName(request.Name);

            if (request.Year is not null)
                property.UpdateYear(request.Year.Value);


            // Save the updated property
            await propertyRepository.SaveAsync(property);
        }
        catch (ArgumentException ex)
        {
            // Re-throw domain validation errors as validation exceptions
            throw new ValidationException($"Domain validation failed: {ex.Message}");
        }
    }
}
using FluentValidation;
using MediatR;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesService.Domain.Properties.Repositories;

namespace Million.PropertiesService.Application.Properties.Commands.AddImageFromProperty;

public sealed class AddImageFromPropertyCommandHandler(
    IPropertyRepository propertyRepository,
    IPropertyImageRepository propertyImageRepository,
    IValidator<AddImageFromPropertyCommand> validator)
    : IRequestHandler<AddImageFromPropertyCommand, AddImageFromPropertyResponse>
{
    public async Task<AddImageFromPropertyResponse> Handle(AddImageFromPropertyCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }

        // Verify that the property exists
        var property = await propertyRepository.GetByIdAsync(request.IdProperty);
        if (property == null)
        {
            throw new ValidationException($"Property with ID {request.IdProperty} not found");
        }

        try
        {
            // Create the property image using the domain method and get the created image
            var createdImage = property.AddImage(request.FileName, request.Enabled);

            // Save the property with the new image
            await propertyImageRepository.SaveAsync(createdImage);

            // Return the response with the created property image ID
            return new AddImageFromPropertyResponse
            {
                IdPropertyImage = createdImage.IdPropertyImage
            };
        }
        catch (ArgumentException ex)
        {
            // Re-throw domain validation errors as validation exceptions
            throw new ValidationException($"Domain validation failed: {ex.Message}");
        }
    }
}

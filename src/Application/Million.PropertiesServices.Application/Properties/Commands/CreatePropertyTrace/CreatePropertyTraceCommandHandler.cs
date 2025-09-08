using FluentValidation;
using MediatR;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesServices.Application.Common.Events;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;

namespace Million.PropertiesServices.Application.Properties.Commands.CreatePropertyTrace;

public sealed class CreatePropertyTraceCommandHandler(
    IPropertyTraceRepository propertyTraceRepository,
    IPropertyRepository propertyRepository,
    IValidator<CreatePropertyTraceCommand> validator,
    IDomainEventDispatcher domainEventDispatcher) 
    : IRequestHandler<CreatePropertyTraceCommand, CreatePropertyTraceResponse>
{
    public async Task<CreatePropertyTraceResponse> Handle(CreatePropertyTraceCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }

        // Verify that the property exists
        if (!await propertyRepository.ExistsAsync(request.PropertyId))
        {
            throw new InvalidOperationException($"Property with ID '{request.PropertyId}' does not exist.");
        }

        try
        {
            // Create the property trace aggregate
            var propertyTrace = PropertyTrace.Create(
                request.PropertyId,
                request.Value,
                request.TaxPercentage
            );

            // Save the property trace
            await propertyTraceRepository.SaveAsync(propertyTrace);

            // Dispatch domain events for eventual consistency
            await domainEventDispatcher.DispatchAsync(propertyTrace.DomainEvents, cancellationToken);
            
            // Clear domain events after publishing
            propertyTrace.ClearDomainEvents();

            // Return the response with the created property trace ID
            return new CreatePropertyTraceResponse
            {
                IdPropertyTrace = propertyTrace.IdPropertyTrace
            };
        }
        catch (ArgumentException ex)
        {
            // Re-throw domain validation errors as validation exceptions
            throw new ValidationException($"Domain validation failed: {ex.Message}");
        }
    }
}
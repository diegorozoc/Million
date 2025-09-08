using MediatR;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.Application.Properties.Commands.UpdateProperty;

public record UpdatePropertyCommand(
    string? Name,
    Address? Address,
    int? Year,
    Guid? IdOwner,
    Guid IdProperty
) : IRequest;
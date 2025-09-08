using MediatR;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.Application.Properties.Commands.ChangePrice;

public record ChangePriceCommand(
    Money Price,
    Guid IdProperty
) : IRequest;

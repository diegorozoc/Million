using MediatR;
using Million.PropertiesServices.Application.Properties.Models;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesServices.Application.Properties.Commands.CreatePropertyBuilding;

public record CreatePropertyBuildingCommand(
    string Name,
    Address Address,
    Money Price,
    string CodeInternal,
    int Year,
    Guid IdOwner
) : IRequest<CreatePropertyBuildingResponse>;

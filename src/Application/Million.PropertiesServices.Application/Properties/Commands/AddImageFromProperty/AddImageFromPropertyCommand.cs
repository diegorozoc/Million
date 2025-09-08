using MediatR;
using Million.PropertiesService.Application.Properties.Models;

namespace Million.PropertiesService.Application.Properties.Commands.AddImageFromProperty;

public record AddImageFromPropertyCommand(
    Guid IdProperty,
    string FileName,
    bool Enabled
) : IRequest<AddImageFromPropertyResponse>;

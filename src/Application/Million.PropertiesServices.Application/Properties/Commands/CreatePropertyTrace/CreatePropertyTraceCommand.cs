using MediatR;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesServices.Application.Properties.Commands.CreatePropertyTrace;

public record CreatePropertyTraceCommand(
    Guid PropertyId,
    Money Value,
    decimal TaxPercentage
) : IRequest<CreatePropertyTraceResponse>;
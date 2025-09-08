using MediatR;
using Million.PropertiesService.Application.Properties.Models;

namespace Million.PropertiesService.Application.Properties.Queries.GetProperties;

public record GetPropertiesQuery(string? Country, string? City, decimal? minPrice, decimal? maxPrice, int? Year) : IRequest<IReadOnlyList<GetPropertiesResponse>>;
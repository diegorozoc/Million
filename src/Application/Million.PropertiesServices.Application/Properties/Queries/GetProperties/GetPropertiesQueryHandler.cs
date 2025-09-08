using AutoMapper;
using MediatR;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Properties.Specifications;

namespace Million.PropertiesService.Application.Properties.Queries.GetProperties;

internal class GetPropertiesQueryHandler : IRequestHandler<GetPropertiesQuery, IReadOnlyList<GetPropertiesResponse>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    public GetPropertiesQueryHandler(
        IPropertyRepository propertyRepository,
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<GetPropertiesResponse>> Handle(GetPropertiesQuery request, CancellationToken cancellationToken)
    {
        // Use specification pattern instead of the legacy method
        var specification = new PropertyFilterSpecification(
            request.Country,
            request.City,
            request.minPrice,
            request.maxPrice,
            request.Year);

        var properties = await _propertyRepository.FindAsync(specification);
        var response = _mapper.Map<IReadOnlyList<GetPropertiesResponse>>(properties);
        return response;
    }
}

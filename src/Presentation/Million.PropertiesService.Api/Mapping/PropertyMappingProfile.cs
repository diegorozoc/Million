using AutoMapper;
using Million.PropertiesService.Api.Models;
using Million.PropertiesServices.Application.Properties.Commands.CreatePropertyBuilding;
using Million.PropertiesService.Application.Properties.Commands.AddImageFromProperty;
using Million.PropertiesService.Application.Properties.Models;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Owners.Entities;

namespace Million.PropertiesService.Api.Mapping;

public class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        CreateMap<NewProperty, CreatePropertyBuildingCommand>()
            .ConstructUsing(src => new CreatePropertyBuildingCommand(
                src.Name,
                src.Address,
                src.Price,
                src.CodeInternal,
                src.Year,
                src.IdOwner
            ));

        CreateMap<NewPropertyImage, AddImageFromPropertyCommand>()
            .ConstructUsing(src => new AddImageFromPropertyCommand(
                src.IdProperty,
                src.FileName,
                src.Enable
            ));

        CreateMap<Owner, OwnerResponse>()
            .ConstructUsing(src => new OwnerResponse(
                src.IdOwner,
                src.Name
            ));

        CreateMap<Property, GetPropertiesResponse>();
    }
}
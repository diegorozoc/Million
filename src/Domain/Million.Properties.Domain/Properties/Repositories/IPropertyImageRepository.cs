using Million.PropertiesService.Domain.Properties.Entities;

namespace Million.PropertiesService.Domain.Properties.Repositories;

public interface IPropertyImageRepository
{
    Task<PropertyImage?> GetByIdAsync(Guid id);
    Task<IEnumerable<PropertyImage>> GetByPropertyIdAsync(Guid propertyId);
    Task<IEnumerable<PropertyImage>> GetEnabledByPropertyIdAsync(Guid propertyId);
    Task<IEnumerable<PropertyImage>> GetByFileNameAsync(string fileName);
    Task<IEnumerable<PropertyImage>> GetImagesByExtensionAsync(string extension);
    Task<IEnumerable<PropertyImage>> GetRecentImagesAsync(int days = 30);
    Task SaveAsync(PropertyImage propertyImage);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> PropertyHasImagesAsync(Guid propertyId);
    Task<int> GetImageCountByPropertyAsync(Guid propertyId);
    Task DeleteByPropertyIdAsync(Guid propertyId);
}
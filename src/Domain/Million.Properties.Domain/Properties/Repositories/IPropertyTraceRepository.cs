using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Common.Specifications;

namespace Million.PropertiesService.Domain.Properties.Repositories;

public interface IPropertyTraceRepository
{
    Task<PropertyTrace?> GetByIdAsync(Guid id);
    
    // Specification-based query methods
    Task<IEnumerable<PropertyTrace>> FindAsync(ISpecification<PropertyTrace> specification);
    Task<PropertyTrace?> FindOneAsync(ISpecification<PropertyTrace> specification);
    Task<int> CountAsync(ISpecification<PropertyTrace> specification);
    
    Task SaveAsync(PropertyTrace propertyTrace);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> PropertyHasTracesAsync(Guid propertyId);
    Task<int> GetTraceCountByPropertyAsync(Guid propertyId);
    Task<Money?> GetAverageValueByPropertyAsync(Guid propertyId);
    Task DeleteByPropertyIdAsync(Guid propertyId);
}
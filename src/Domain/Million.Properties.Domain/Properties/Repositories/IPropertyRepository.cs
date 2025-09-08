using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Common.Specifications;

namespace Million.PropertiesService.Domain.Properties.Repositories;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id);
    
    // Specification-based query methods
    Task<IEnumerable<Property>> FindAsync(ISpecification<Property> specification);
    Task<Property?> FindOneAsync(ISpecification<Property> specification);
    Task<int> CountAsync(ISpecification<Property> specification);
    
    Task SaveAsync(Property property);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CodeInternalExistsAsync(string codeInternal);
}
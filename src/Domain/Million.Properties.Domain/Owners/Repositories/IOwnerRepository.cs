using Million.PropertiesService.Domain.Owners.Entities;

namespace Million.PropertiesService.Domain.Owners.Repositories;

public interface IOwnerRepository
{
    Task<Owner?> GetByIdAsync(Guid id);
    Task<Owner?> GetByNameAsync(string name);
    Task<IEnumerable<Owner>> GetOwnersWithPropertiesAsync();
    Task<IEnumerable<Owner>> GetAdultOwnersAsync();
    Task<IEnumerable<Owner>> GetOwnersByAgeRangeAsync(int minAge, int maxAge);
    Task SaveAsync(Owner owner);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetTotalPropertyCountAsync(Guid ownerId);
}
using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Owners.Entities;
using Million.PropertiesService.Domain.Owners.Repositories;

namespace Million.PropertiesService.Persistance.Owners.Repositories;

public class OwnerRepository : IOwnerRepository
{
    private readonly PropertiesDbContext _context;

    public OwnerRepository(PropertiesDbContext context)
    {
        _context = context;
    }

    public async Task<Owner?> GetByIdAsync(Guid id)
    {
        return await _context.Owners
            .FirstOrDefaultAsync(o => o.IdOwner == id);
    }

    public async Task<Owner?> GetByNameAsync(string name)
    {
        return await _context.Owners
            .FirstOrDefaultAsync(o => o.Name == name);
    }

    public async Task<IEnumerable<Owner>> GetOwnersWithPropertiesAsync()
    {
        return await _context.Owners
            .Where(o => o.PropertyIds.Any())
            .ToListAsync();
    }

    public async Task<IEnumerable<Owner>> GetAdultOwnersAsync()
    {
        return await _context.Owners
            .Where(o => o.DateOfBirth.IsAdult())
            .ToListAsync();
    }

    public async Task<IEnumerable<Owner>> GetOwnersByAgeRangeAsync(int minAge, int maxAge)
    {
        var today = DateTime.Today;
        var maxBirthDate = today.AddYears(-minAge);
        var minBirthDate = today.AddYears(-maxAge - 1);

        return await _context.Owners
            .Where(o => o.DateOfBirth.Value >= minBirthDate && o.DateOfBirth.Value <= maxBirthDate)
            .ToListAsync();
    }

    public async Task SaveAsync(Owner owner)
    {
        if (await ExistsAsync(owner.IdOwner))
        {
            _context.Owners.Update(owner);
        }
        else
        {
            await _context.Owners.AddAsync(owner);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var owner = await GetByIdAsync(id);
        if (owner != null)
        {
            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Owners
            .AnyAsync(o => o.IdOwner == id);
    }

    public async Task<int> GetTotalPropertyCountAsync(Guid ownerId)
    {
        var owner = await GetByIdAsync(ownerId);
        return owner?.GetPropertyCount() ?? 0;
    }
}
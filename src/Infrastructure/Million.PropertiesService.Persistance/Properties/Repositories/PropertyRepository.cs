using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Common.Specifications;
using Million.PropertiesService.Persistance.Common;

namespace Million.PropertiesService.Persistance.Properties.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly PropertiesDbContext _context;

    public PropertyRepository(PropertiesDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        return await _context.Properties
            .FirstOrDefaultAsync(p => p.IdProperty == id);
    }

    public async Task<IEnumerable<Property>> FindAsync(ISpecification<Property> specification)
    {
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        return await query.ToListAsync();
    }

    public async Task<Property?> FindOneAsync(ISpecification<Property> specification)
    {
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<int> CountAsync(ISpecification<Property> specification)
    {
        var query = SpecificationEvaluator.GetQuery(_context.Properties.AsQueryable(), specification);
        return await query.CountAsync();
    }


    public async Task SaveAsync(Property property)
    {
        if (await ExistsAsync(property.IdProperty))
        {
            _context.Properties.Update(property);
        }
        else
        {
            await _context.Properties.AddAsync(property);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var property = await GetByIdAsync(id);
        if (property != null)
        {
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Properties
            .AnyAsync(p => p.IdProperty == id);
    }

    public async Task<bool> CodeInternalExistsAsync(string codeInternal)
    {
        return await _context.Properties
            .AnyAsync(p => p.CodeInternal == codeInternal);
    }
}
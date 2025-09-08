using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Common.Specifications;
using Million.PropertiesService.Persistance.Common;

namespace Million.PropertiesService.Persistance.Properties.Repositories;

public class PropertyTraceRepository : IPropertyTraceRepository
{
    private readonly PropertiesDbContext _context;

    public PropertyTraceRepository(PropertiesDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyTrace?> GetByIdAsync(Guid id)
    {
        return await _context.PropertyTraces
            .FirstOrDefaultAsync(pt => pt.IdPropertyTrace == id);
    }

    public async Task<IEnumerable<PropertyTrace>> FindAsync(ISpecification<PropertyTrace> specification)
    {
        var query = SpecificationEvaluator.GetQuery(_context.PropertyTraces.AsQueryable(), specification);
        return await query.ToListAsync();
    }

    public async Task<PropertyTrace?> FindOneAsync(ISpecification<PropertyTrace> specification)
    {
        var query = SpecificationEvaluator.GetQuery(_context.PropertyTraces.AsQueryable(), specification);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<int> CountAsync(ISpecification<PropertyTrace> specification)
    {
        var query = SpecificationEvaluator.GetQuery(_context.PropertyTraces.AsQueryable(), specification);
        return await query.CountAsync();
    }


    public async Task SaveAsync(PropertyTrace propertyTrace)
    {
        if (await ExistsAsync(propertyTrace.IdPropertyTrace))
        {
            _context.PropertyTraces.Update(propertyTrace);
        }
        else
        {
            await _context.PropertyTraces.AddAsync(propertyTrace);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var propertyTrace = await GetByIdAsync(id);
        if (propertyTrace != null)
        {
            _context.PropertyTraces.Remove(propertyTrace);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.PropertyTraces
            .AnyAsync(pt => pt.IdPropertyTrace == id);
    }

    public async Task<bool> PropertyHasTracesAsync(Guid propertyId)
    {
        return await _context.PropertyTraces
            .AnyAsync(pt => pt.IdProperty == propertyId);
    }

    public async Task<int> GetTraceCountByPropertyAsync(Guid propertyId)
    {
        return await _context.PropertyTraces
            .CountAsync(pt => pt.IdProperty == propertyId);
    }

    public async Task<Money?> GetAverageValueByPropertyAsync(Guid propertyId)
    {
        var traces = await _context.PropertyTraces
            .Where(pt => pt.IdProperty == propertyId)
            .ToListAsync();

        if (!traces.Any())
            return null;

        var averageAmount = traces.Average(pt => pt.Value.Amount);
        var currency = traces.First().Value.Currency;
        
        return new Money(averageAmount, currency);
    }

    public async Task DeleteByPropertyIdAsync(Guid propertyId)
    {
        var traces = await _context.PropertyTraces
            .Where(pt => pt.IdProperty == propertyId)
            .ToListAsync();
        
        if (traces.Any())
        {
            _context.PropertyTraces.RemoveRange(traces);
            await _context.SaveChangesAsync();
        }
    }
}
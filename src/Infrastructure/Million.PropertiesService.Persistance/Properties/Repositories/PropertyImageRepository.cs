using Microsoft.EntityFrameworkCore;
using Million.PropertiesService.Domain.Properties.Entities;
using Million.PropertiesService.Domain.Properties.Repositories;

namespace Million.PropertiesService.Persistance.Properties.Repositories;

public class PropertyImageRepository : IPropertyImageRepository
{
    private readonly PropertiesDbContext _context;

    public PropertyImageRepository(PropertiesDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyImage?> GetByIdAsync(Guid id)
    {
        return await _context.PropertyImages
            .FirstOrDefaultAsync(pi => pi.IdPropertyImage == id);
    }

    public async Task<IEnumerable<PropertyImage>> GetByPropertyIdAsync(Guid propertyId)
    {
        return await _context.PropertyImages
            .Where(pi => pi.IdProperty == propertyId)
            .OrderBy(pi => pi.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropertyImage>> GetEnabledByPropertyIdAsync(Guid propertyId)
    {
        return await _context.PropertyImages
            .Where(pi => pi.IdProperty == propertyId && pi.Enabled)
            .OrderBy(pi => pi.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropertyImage>> GetByFileNameAsync(string fileName)
    {
        return await _context.PropertyImages
            .Where(pi => pi.FileName.Contains(fileName))
            .OrderBy(pi => pi.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropertyImage>> GetImagesByExtensionAsync(string extension)
    {
        var normalizedExtension = extension.StartsWith('.') ? extension : $".{extension}";
        return await _context.PropertyImages
            .Where(pi => pi.FileName.EndsWith(normalizedExtension))
            .OrderBy(pi => pi.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropertyImage>> GetRecentImagesAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.PropertyImages
            .Where(pi => pi.CreatedAt >= cutoffDate)
            .OrderByDescending(pi => pi.CreatedAt)
            .ToListAsync();
    }

    public async Task SaveAsync(PropertyImage propertyImage)
    {
        if (await ExistsAsync(propertyImage.IdPropertyImage))
        {
            _context.PropertyImages.Update(propertyImage);
        }
        else
        {
            await _context.PropertyImages.AddAsync(propertyImage);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var propertyImage = await GetByIdAsync(id);
        if (propertyImage != null)
        {
            _context.PropertyImages.Remove(propertyImage);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.PropertyImages
            .AnyAsync(pi => pi.IdPropertyImage == id);
    }

    public async Task<bool> PropertyHasImagesAsync(Guid propertyId)
    {
        return await _context.PropertyImages
            .AnyAsync(pi => pi.IdProperty == propertyId);
    }

    public async Task<int> GetImageCountByPropertyAsync(Guid propertyId)
    {
        return await _context.PropertyImages
            .CountAsync(pi => pi.IdProperty == propertyId);
    }

    public async Task DeleteByPropertyIdAsync(Guid propertyId)
    {
        var images = await _context.PropertyImages
            .Where(pi => pi.IdProperty == propertyId)
            .ToListAsync();
        
        if (images.Any())
        {
            _context.PropertyImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }
    }
}
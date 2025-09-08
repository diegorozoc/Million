namespace Million.PropertiesService.Domain.Properties.Entities;

public class PropertyImage
{
    public Guid IdPropertyImage { get; private set; }
    public Guid IdProperty { get; private set; }
    public string FileName { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private PropertyImage() { }

    private PropertyImage(Guid propertyId, string fileName, bool enabled = true)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        IdPropertyImage = Guid.NewGuid();
        IdProperty = propertyId;
        FileName = fileName;
        Enabled = enabled;
        CreatedAt = DateTime.UtcNow;
    }

    public static PropertyImage Create(Guid propertyId, string fileName, bool enabled = true)
    {
        return new PropertyImage(propertyId, fileName, enabled);
    }

    public void Enable()
    {
        if (!Enabled)
        {
            Enabled = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Disable()
    {
        if (Enabled)
        {
            Enabled = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateFileName(string newFileName)
    {
        if (string.IsNullOrWhiteSpace(newFileName))
            throw new ArgumentException("File name cannot be empty", nameof(newFileName));
        
        FileName = newFileName;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetFileExtension()
    {
        return System.IO.Path.GetExtension(FileName);
    }

    public bool IsImageFile()
    {
        var extension = GetFileExtension().ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp";
    }
}

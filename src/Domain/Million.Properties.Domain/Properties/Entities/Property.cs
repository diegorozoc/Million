using Million.PropertiesService.Domain.Common.Entities;
using Million.PropertiesService.Domain.Common.ValueObjects;
using Million.PropertiesService.Domain.Properties.Events;
using Million.PropertiesService.Domain.Owners.Entities;

namespace Million.PropertiesService.Domain.Properties.Entities;

public class Property : AggregateRoot
{
    private readonly List<PropertyImage> _images = new();

    public Guid IdProperty { get; private set; }
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public Money Price { get; private set; }
    public string CodeInternal { get; private set; }
    public int Year { get; private set; }
    public Guid IdOwner { get; private set; }
    public Owner Owner { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<PropertyImage> Images => _images.AsReadOnly();

    private Property() { }

    private Property(string name, Address address, Money price, string codeInternal, int year, Owner owner)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(codeInternal))
            throw new ArgumentException("Code internal cannot be empty", nameof(codeInternal));
        if (year < 1800 || year > DateTime.Now.Year)
            throw new ArgumentException("Year must be between 1800 and current year", nameof(year));

        IdProperty = Guid.NewGuid();
        Name = name;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Price = price ?? throw new ArgumentNullException(nameof(price));
        CodeInternal = codeInternal;
        Year = year;
        SetOwner(owner);
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PropertyCreated(IdProperty, Name, Address, Price, IdOwner));
    }

    public static Property Create(string name, Address address, Money price, string codeInternal, int year, Owner owner)
    {
        return new Property(name, address, price, codeInternal, year, owner);
    }

    public void SetOwner(Owner owner)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        IdOwner = owner.IdOwner;
    }

    public void ChangePrice(Money newPrice)
    {
        if (newPrice == null)
            throw new ArgumentNullException(nameof(newPrice));
        
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PropertyPriceChanged(IdProperty, newPrice));
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));
        
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address newAddress)
    {
        Address = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateYear(int newYear)
    {
        if (newYear < 1800 || newYear > DateTime.Now.Year)
            throw new ArgumentException("Year must be between 1800 and current year", nameof(newYear));
        Year = newYear;
        UpdatedAt = DateTime.UtcNow;
    }

    public PropertyImage AddImage(string fileName, bool enabled = true)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        var image = PropertyImage.Create(IdProperty, fileName, enabled);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;
        
        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.IdPropertyImage == imageId);
        if (image != null)
        {
            _images.Remove(image);
            UpdatedAt = DateTime.UtcNow;
        }
    }


    public bool HasImages() => _images.Any();
    public bool HasActiveImages() => _images.Any(i => i.Enabled);
}
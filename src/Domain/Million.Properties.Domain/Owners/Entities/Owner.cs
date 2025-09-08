using Million.PropertiesService.Domain.Common.Entities;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesService.Domain.Owners.Entities;

public class Owner : AggregateRoot
{
    private readonly List<Guid> _propertyIds = new();

    public Guid IdOwner { get; private set; }
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public string? PhotoUrl { get; private set; }
    public DateOfBirth DateOfBirth { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<Guid> PropertyIds => _propertyIds.AsReadOnly();


    private Owner() { }


    private Owner(string name, Address address, DateOfBirth dateOfBirth, string? photoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        IdOwner = Guid.NewGuid();
        Name = name;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        DateOfBirth = dateOfBirth ?? throw new ArgumentNullException(nameof(dateOfBirth));
        PhotoUrl = photoUrl;
        CreatedAt = DateTime.UtcNow;
    }

    public static Owner Create(string name, Address address, DateOfBirth dateOfBirth, string? photoUrl = null)
    {
        return new Owner(name, address, dateOfBirth, photoUrl);
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

    public void UpdatePhoto(string? newPhotoUrl)
    {
        PhotoUrl = newPhotoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddProperty(Guid propertyId)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));
        
        if (!_propertyIds.Contains(propertyId))
        {
            _propertyIds.Add(propertyId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveProperty(Guid propertyId)
    {
        if (_propertyIds.Remove(propertyId))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool IsAdult() => DateOfBirth.IsAdult();
    public int GetAge() => DateOfBirth.GetAge();
    public bool HasProperties() => _propertyIds.Any();
    public int GetPropertyCount() => _propertyIds.Count;

    public bool CanOwnMoreProperties(int maxProperties = 10)
    {
        return _propertyIds.Count < maxProperties;
    }
}
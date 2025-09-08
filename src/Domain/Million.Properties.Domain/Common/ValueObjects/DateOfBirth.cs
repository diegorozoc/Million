namespace Million.PropertiesService.Domain.Common.ValueObjects;

public record DateOfBirth
{
    public DateTime Value { get; }

    public DateOfBirth(DateTime value)
    {
        if (value > DateTime.Today)
            throw new ArgumentException("Date of birth cannot be in the future", nameof(value));
        if (value < DateTime.Today.AddYears(-150))
            throw new ArgumentException("Date of birth cannot be more than 150 years ago", nameof(value));

        Value = value;
    }

    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - Value.Year;
        if (Value.Date > today.AddYears(-age)) age--;
        return age;
    }

    public bool IsAdult() => GetAge() >= 18;
}
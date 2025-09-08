using FluentAssertions;
using Million.PropertiesService.Domain.Common.ValueObjects;

namespace Million.PropertiesServices.Application.UnitTests;

[TestFixture]
public class BasicApplicationTests
{
    [Test]
    public void Money_Constructor_ShouldCreateValidMoney()
    {
        // Arrange
        var amount = 100000m;
        var currency = "USD";

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Should().NotBeNull();
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Test]
    public void Address_Constructor_ShouldCreateValidAddress()
    {
        // Arrange
        var street = "123 Test St";
        var city = "Test City";
        var postalCode = "12345";
        var country = "USA";

        // Act
        var address = new Address(street, city, postalCode, country);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be(street);
        address.City.Should().Be(city);
        address.PostalCode.Should().Be(postalCode);
        address.Country.Should().Be(country);
    }

    [Test]
    public void DateOfBirth_Constructor_ShouldCreateValidDateOfBirth()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-25);

        // Act
        var dateOfBirth = new DateOfBirth(birthDate);

        // Assert
        dateOfBirth.Should().NotBeNull();
        dateOfBirth.Value.Should().Be(birthDate);
    }
}
using FluentValidation;

namespace Million.PropertiesServices.Application.Properties.Commands.CreatePropertyBuilding;

public class CreatePropertyBuildingCommandValidator : AbstractValidator<CreatePropertyBuildingCommand>
{
    public CreatePropertyBuildingCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Property name is required")
            .Length(1, 200).WithMessage("Property name must be between 1 and 200 characters");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address is required");

        RuleFor(x => x.Address.Street)
            .NotEmpty().WithMessage("Street is required")
            .Length(1, 200).WithMessage("Street must be between 1 and 200 characters");

        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("City is required")
            .Length(1, 100).WithMessage("City must be between 1 and 100 characters");

        RuleFor(x => x.Address.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .Length(1, 20).WithMessage("Postal code must be between 1 and 20 characters");

        RuleFor(x => x.Address.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(1, 100).WithMessage("Country must be between 1 and 100 characters");

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Price is required");

        RuleFor(x => x.Price.Amount)
            .GreaterThan(0).WithMessage("Price amount must be greater than zero");

        RuleFor(x => x.Price.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be exactly 3 characters (e.g., USD, EUR)");

        RuleFor(x => x.CodeInternal)
            .NotEmpty().WithMessage("Internal code is required")
            .Length(1, 50).WithMessage("Internal code must be between 1 and 50 characters");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(1800).WithMessage("Year must be 1800 or later")
            .LessThanOrEqualTo(DateTime.Now.Year).WithMessage($"Year cannot be in the future (max: {DateTime.Now.Year})");

        RuleFor(x => x.IdOwner)
            .NotEmpty().WithMessage("Owner ID is required");
    }
}
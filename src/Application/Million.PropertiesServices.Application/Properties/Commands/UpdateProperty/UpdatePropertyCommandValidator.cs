using FluentValidation;

namespace Million.PropertiesService.Application.Properties.Commands.UpdateProperty;

public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
            .Length(1, 200).WithMessage("Property name must be between 1 and 200 characters");
        });

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address!.Street)
                .Length(1, 200).WithMessage("Street must be between 1 and 200 characters");

            RuleFor(x => x.Address!.City)
                .Length(1, 100).WithMessage("City must be between 1 and 100 characters");

            RuleFor(x => x.Address!.PostalCode)
                .Length(1, 20).WithMessage("Postal code must be between 1 and 20 characters");

            RuleFor(x => x.Address!.Country)
                .Length(1, 100).WithMessage("Country must be between 1 and 100 characters");
        });

        When(x => x.Year.HasValue, () =>
        {
            RuleFor(x => x.Year!.Value)
                .GreaterThanOrEqualTo(1800).WithMessage("Year must be 1800 or later")
                .LessThanOrEqualTo(DateTime.Now.Year).WithMessage($"Year cannot be in the future (max: {DateTime.Now.Year})");
        });
    }
}
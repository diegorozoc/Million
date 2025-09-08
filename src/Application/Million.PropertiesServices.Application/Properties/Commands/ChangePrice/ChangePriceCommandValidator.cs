using FluentValidation;

namespace Million.PropertiesService.Application.Properties.Commands.ChangePrice;

public class ChangePriceCommandValidator : AbstractValidator<ChangePriceCommand>
{
    public ChangePriceCommandValidator()
    {

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Price is required");

        RuleFor(x => x.Price.Amount)
            .GreaterThan(0).WithMessage("Price amount must be greater than zero");

        RuleFor(x => x.Price.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be exactly 3 characters (e.g., USD, EUR)");
    }
}
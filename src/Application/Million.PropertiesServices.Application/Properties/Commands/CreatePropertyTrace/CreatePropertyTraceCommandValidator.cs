using FluentValidation;

namespace Million.PropertiesServices.Application.Properties.Commands.CreatePropertyTrace;

public sealed class CreatePropertyTraceCommandValidator : AbstractValidator<CreatePropertyTraceCommand>
{
    public CreatePropertyTraceCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required.");

        RuleFor(x => x.Value)
            .NotNull()
            .WithMessage("Value is required.");

        RuleFor(x => x.Value.Amount)
            .GreaterThan(0)
            .WithMessage("Value amount must be greater than 0.")
            .When(x => x.Value != null);

        RuleFor(x => x.TaxPercentage)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Tax percentage must be between 0 and 100.");
    }
}
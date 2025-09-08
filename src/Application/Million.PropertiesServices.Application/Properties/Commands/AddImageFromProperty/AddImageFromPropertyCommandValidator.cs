using FluentValidation;

namespace Million.PropertiesService.Application.Properties.Commands.AddImageFromProperty;

public class AddImageFromPropertyCommandValidator : AbstractValidator<AddImageFromPropertyCommand>
{
    public AddImageFromPropertyCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .Length(1, 255).WithMessage("File name must be between 1 and 255 characters");

        RuleFor(x => x.IdProperty)
            .NotEmpty().WithMessage("Property ID is required");
    }
}
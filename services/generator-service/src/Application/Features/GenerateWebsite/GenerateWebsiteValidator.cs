using FluentValidation;
using GeneratorService.Application.Features.GenerateWebsite;

namespace GeneratorService.Application.Features.GenerateWebsite;

/// <summary>
/// Validator for GenerateWebsiteCommand
/// </summary>
public class GenerateWebsiteValidator : AbstractValidator<GenerateWebsiteCommand>
{
    public GenerateWebsiteValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .Length(3, 100).WithMessage("Project name must be between 3 and 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(10, 500).WithMessage("Description must be between 10 and 500 characters");

        RuleFor(x => x.Industry)
            .NotEmpty().WithMessage("Industry is required")
            .Length(3, 50).WithMessage("Industry must be between 3 and 50 characters");

        RuleFor(x => x.Features)
            .NotEmpty().WithMessage("At least one feature is required")
            .Must(f => f.All(feature => !string.IsNullOrWhiteSpace(feature)))
            .WithMessage("All features must be non-empty");

        RuleFor(x => x.ColorScheme)
            .NotEmpty().WithMessage("Color scheme is required");
    }
}

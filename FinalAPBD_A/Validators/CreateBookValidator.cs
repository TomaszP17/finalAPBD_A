using FinalAPBD_A.DTOs;
using FluentValidation;

namespace FinalAPBD_A.Validators;

public class CreateBookValidator : AbstractValidator<AddBookDto>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Title Book must be 1-100 lenght");
    }
}
using Api.DTOs;
using FluentValidation;

namespace Api.Validators;

public sealed class GetMessageRequestValidator : AbstractValidator<GetMessageRequest>
{
    public GetMessageRequestValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .Matches(@"^\d{1,6}$")
                .WithMessage("A senha deve conter apenas dígitos numéricos e ter entre 1 e 6 caracteres.");
        });
    }
}

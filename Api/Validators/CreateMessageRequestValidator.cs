using Api.DTOs;
using FluentValidation;

namespace Api.Validators;

public sealed class CreateMessageRequestValidator : AbstractValidator<CreateMessageRequest>
{
    private static readonly string[] AllowedExpirations = ["12h", "7d", "1m"];

    public CreateMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("O conteúdo da mensagem é obrigatório.")
            .MaximumLength(50_000)
            .WithMessage("O conteúdo da mensagem não pode exceder 50.000 caracteres.");

        RuleFor(x => x.Expiration)
            .NotEmpty()
            .WithMessage("O tempo de expiração é obrigatório.")
            .Must(e => AllowedExpirations.Contains(e))
            .WithMessage("O tempo de expiração deve ser '12h', '7d' ou '1m'.");

        When(x => !string.IsNullOrEmpty(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .Matches(@"^\d{1,6}$")
                .WithMessage("A senha deve conter apenas dígitos numéricos e ter entre 1 e 6 caracteres.");
        });
    }
}

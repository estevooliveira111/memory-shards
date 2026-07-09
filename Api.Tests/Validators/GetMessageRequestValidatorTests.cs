using Api.DTOs;
using Api.Validators;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators;

public class GetMessageRequestValidatorTests
{
    private readonly GetMessageRequestValidator _sut = new();

    [Fact]
    public void NullPassword_PassesValidation()
    {
        var result = _sut.TestValidate(new GetMessageRequest { Password = null });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyPassword_PassesValidation()
    {
        var result = _sut.TestValidate(new GetMessageRequest { Password = "" });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123456")]
    public void NumericPasswordWithinLength_PassesValidation(string password)
    {
        var result = _sut.TestValidate(new GetMessageRequest { Password = password });

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("abcdef")]
    [InlineData("1234567")]
    [InlineData("12 34")]
    public void InvalidPassword_FailsValidation(string password)
    {
        var result = _sut.TestValidate(new GetMessageRequest { Password = password });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

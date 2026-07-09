using Api.DTOs;
using Api.Validators;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators;

public class CreateMessageRequestValidatorTests
{
    private readonly CreateMessageRequestValidator _sut = new();

    [Fact]
    public void ValidRequest_WithoutPassword_PassesValidation()
    {
        var request = new CreateMessageRequest { Content = "olá", Expiration = "12h" };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidRequest_WithNumericPassword_PassesValidation()
    {
        var request = new CreateMessageRequest { Content = "olá", Expiration = "7d", Password = "1234" };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyContent_FailsValidation()
    {
        var request = new CreateMessageRequest { Content = "", Expiration = "12h" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void ContentExceeding50000Characters_FailsValidation()
    {
        var request = new CreateMessageRequest { Content = new string('a', 50_001), Expiration = "12h" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void ContentAtExactly50000Characters_PassesValidation()
    {
        var request = new CreateMessageRequest { Content = new string('a', 50_000), Expiration = "12h" };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void EmptyExpiration_FailsValidation()
    {
        var request = new CreateMessageRequest { Content = "olá", Expiration = "" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Expiration);
    }

    [Theory]
    [InlineData("1h")]
    [InlineData("30d")]
    [InlineData("1y")]
    [InlineData("12H")]
    public void InvalidExpirationValue_FailsValidation(string expiration)
    {
        var request = new CreateMessageRequest { Content = "olá", Expiration = expiration };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Expiration);
    }

    [Theory]
    [InlineData("abcdef")]
    [InlineData("12345678")]
    [InlineData("12 34")]
    [InlineData("-123")]
    [InlineData(" ")]
    public void NonNumericOrTooLongPassword_FailsValidation(string password)
    {
        var request = new CreateMessageRequest { Content = "olá", Expiration = "12h", Password = password };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123456")]
    public void NumericPasswordWithinLength_PassesValidation(string password)
    {
        var request = new CreateMessageRequest { Content = "olá", Expiration = "12h", Password = password };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void NullPassword_IsTreatedAsUnprotectedAndPassesValidation()
    {
        var request = new CreateMessageRequest { Content = "olá", Expiration = "12h", Password = null };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}

using Api.Entities;

namespace Api.Tests.Entities;

public class TemporaryMessageTests
{
    [Fact]
    public void IsExpired_WhenExpiresAtIsInThePast_ReturnsTrue()
    {
        var message = new TemporaryMessage { ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };

        Assert.True(message.IsExpired());
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInTheFuture_ReturnsFalse()
    {
        var message = new TemporaryMessage { ExpiresAt = DateTime.UtcNow.AddMinutes(1) };

        Assert.False(message.IsExpired());
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsExactlyNow_ReturnsTrue()
    {
        var message = new TemporaryMessage { ExpiresAt = DateTime.UtcNow };

        Assert.True(message.IsExpired());
    }
}

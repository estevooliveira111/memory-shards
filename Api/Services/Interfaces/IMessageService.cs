using Api.DTOs;

namespace Api.Services.Interfaces;

public interface IMessageService
{
    /// <summary>Creates a new temporary message and returns share information.</summary>
    Task<CreateMessageResponse> CreateAsync(CreateMessageRequest request, CancellationToken ct = default);

    /// <summary>
    /// Retrieves and decrypts (if needed) a message by slug.
    /// </summary>
    /// <param name="password">Optional password. Required for encrypted messages.</param>
    /// <exception cref="Api.Exceptions.MessageNotFoundException"/>
    /// <exception cref="Api.Exceptions.MessageExpiredException"/>
    /// <exception cref="Api.Exceptions.InvalidPasswordException"/>
    Task<GetMessageResponse> GetBySlugAsync(string slug, string? password, CancellationToken ct = default);

    /// <summary>Deletes a message by slug (admin operation).</summary>
    /// <exception cref="Api.Exceptions.MessageNotFoundException"/>
    Task DeleteBySlugAsync(string slug, CancellationToken ct = default);
}

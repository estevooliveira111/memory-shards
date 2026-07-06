using Api.Entities;

namespace Api.Repositories.Interfaces;

public interface IMessageRepository
{
    /// <summary>Finds a message by its slug. Returns null if not found.</summary>
    Task<TemporaryMessage?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Checks whether a slug is already in use.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);

    /// <summary>Persists a new message and returns its generated slug.</summary>
    Task<string> AddAsync(TemporaryMessage message, CancellationToken ct = default);

    /// <summary>Removes a message from the store.</summary>
    Task DeleteAsync(TemporaryMessage message, CancellationToken ct = default);

    /// <summary>
    /// Deletes all messages whose ExpiresAt is in the past.
    /// Returns the count of deleted rows.
    /// </summary>
    Task<int> DeleteExpiredAsync(CancellationToken ct = default);
}

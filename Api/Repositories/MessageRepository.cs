using Api.Entities;
using Api.Infrastructure.Data;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public sealed class MessageRepository(AppDbContext db) : IMessageRepository
{
    public Task<TemporaryMessage?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => db.Messages
             .AsNoTracking()
             .FirstOrDefaultAsync(m => m.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        => db.Messages.AnyAsync(m => m.Slug == slug, ct);

    public async Task<string> AddAsync(TemporaryMessage message, CancellationToken ct = default)
    {
        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);
        return message.Slug;
    }

    public async Task DeleteAsync(TemporaryMessage message, CancellationToken ct = default)
    {
        // Re-attach if the entity was loaded with AsNoTracking
        db.Messages.Remove(message);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // ExecuteDeleteAsync issues a single DELETE SQL — efficient for large tables
        return await db.Messages
            .Where(m => m.ExpiresAt <= now)
            .ExecuteDeleteAsync(ct);
    }
}

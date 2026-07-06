using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TemporaryMessage> Messages => Set<TemporaryMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TemporaryMessage>(entity =>
        {
            entity.ToTable("TemporaryMessages");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(32);

            entity.Property(e => e.EncryptedContent)
                .IsRequired();

            entity.Property(e => e.IsEncrypted)
                .IsRequired();

            entity.Property(e => e.Salt)
                .HasMaxLength(128);

            entity.Property(e => e.IV)
                .HasMaxLength(128);

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Unique index on Slug for fast lookups and uniqueness enforcement
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_TemporaryMessages_Slug");

            // Index on ExpiresAt for efficient cleanup queries
            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_TemporaryMessages_ExpiresAt");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<TemporaryMessage>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

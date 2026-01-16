using Algocura.Platform.Auditing.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Platform.Auditing.Tests.Support;

internal sealed class TestDbContext(
    DbContextOptions<TestDbContext> options)
    : DbContext(options)
{
    public DbSet<TestEntity> Entities =>
        Set<TestEntity>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(
            entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .ValueGeneratedNever();
            });
    }
}

internal sealed class TestEntity
    : IAuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public string? Password { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}


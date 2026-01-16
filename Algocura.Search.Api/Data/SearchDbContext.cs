using Algocura.Platform.Auditing.Abstractions;
using Algocura.Search.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Search.Api.Data;

public sealed class SearchDbContext(
    DbContextOptions<SearchDbContext> options)
    : DbContext(options)
{
    public DbSet<SearchDocument> SearchDocuments =>
        Set<SearchDocument>();

    public DbSet<SearchDocumentTag> SearchDocumentTags =>
        Set<SearchDocumentTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchDocument>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Score).HasPrecision(18, 6);

            entity.HasIndex(x => new
            {
                x.DocumentType,
                x.SourceEntityId
            }).IsUnique();

            entity.HasIndex(x => x.Title);
            entity.HasIndex(x => x.DiseaseName);
            entity.HasIndex(x => x.GeneSymbol);
            entity.HasIndex(x => x.CompoundName);
            entity.HasIndex(x => x.InchiKey);
            entity.HasIndex(x => x.WorkflowId);
            entity.HasIndex(x => x.Status);

            entity.HasMany(x => x.Tags)
                .WithOne(x => x.SearchDocument)
                .HasForeignKey(x => x.SearchDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SearchDocumentTag>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.HasIndex(x => new
            {
                x.SearchDocumentId,
                x.Name,
                x.Value
            }).IsUnique();
        });

        ConfigureAuditableEntities(modelBuilder);
    }

    private static void ConfigureAuditableEntities(
        ModelBuilder modelBuilder)
    {
        var auditableEntityTypes =
            modelBuilder.Model
                .GetEntityTypes()
                .Where(entityType =>
                    typeof(IAuditableEntity)
                        .IsAssignableFrom(entityType.ClrType))
                .ToArray();

        foreach (var entityType in auditableEntityTypes)
        {
            var entity =
                modelBuilder.Entity(entityType.ClrType);

            entity.Property<Guid?>(
                    nameof(IAuditableEntity.CreatedByUserId))
                .IsRequired(false);

            entity.Property<Guid?>(
                    nameof(IAuditableEntity.UpdatedByUserId))
                .IsRequired(false);

            entity.Property<DateTimeOffset>(
                    nameof(IAuditableEntity.CreatedAtUtc))
                .IsRequired();

            entity.Property<DateTimeOffset>(
                    nameof(IAuditableEntity.UpdatedAtUtc))
                .IsRequired();

            entity.HasIndex(
                nameof(IAuditableEntity.CreatedByUserId));

            entity.HasIndex(
                nameof(IAuditableEntity.UpdatedByUserId));
        }
    }
}

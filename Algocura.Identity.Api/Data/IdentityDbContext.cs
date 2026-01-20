using Algocura.Identity.Api.Domain;
using Algocura.Platform.Auditing.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Identity.Api.Data;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<ApplicationRole>()
            .ToTable("Roles");

        modelBuilder.Entity<IdentityUserRole<Guid>>()
            .ToTable("UserRoles");

        modelBuilder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("UserClaims");

        modelBuilder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("UserLogins");

        modelBuilder.Entity<IdentityRoleClaim<Guid>>()
            .ToTable("RoleClaims");

        modelBuilder.Entity<IdentityUserToken<Guid>>()
            .ToTable("UserTokens");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(x => typeof(IAuditableEntity).IsAssignableFrom(x.ClrType)))
        {
            var entity = modelBuilder.Entity(entityType.ClrType);

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

            entity.HasIndex(nameof(IAuditableEntity.CreatedByUserId));
            entity.HasIndex(nameof(IAuditableEntity.UpdatedByUserId));
        }
    }
}

using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Platform.Auditing.Models;
using Algocura.Platform.Auditing.Options;
using Algocura.Platform.Auditing.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Algocura.Platform.Auditing.Tests.Interceptors;

public sealed class AuditableEntityInterceptorTests
{
    [Fact]
    public async Task SaveChanges_NewEntity_StampsAuditMetadata()
    {
        var userId =
            Guid.NewGuid();

        var currentUser =
            Substitute.For<ICurrentUserContext>();

        currentUser.UserId
            .Returns(userId);

        currentUser.ActorType
            .Returns(AuditActorType.User);

        var interceptor =
            new AuditableEntityInterceptor(
                currentUser,
                Microsoft.Extensions.Options.Options.Create(
                    new AuditingOptions()));

        await using var connection =
            new Microsoft.Data.Sqlite.SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(interceptor)
                .Options;

        await using var dbContext =
            new TestDbContext(options);

        await dbContext.Database
            .EnsureCreatedAsync();

        var entity =
            new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Entity"
            };

        dbContext.Entities.Add(entity);

        await dbContext.SaveChangesAsync();

        entity.CreatedByUserId.Should()
            .Be(userId);

        entity.UpdatedByUserId.Should()
            .Be(userId);

        entity.CreatedAtUtc.Should()
            .NotBe(default);

        entity.UpdatedAtUtc.Should()
            .NotBe(default);
    }

    [Fact]
    public async Task SaveChanges_ModifiedEntity_PreservesCreatedMetadata()
    {
        var originalUserId =
            Guid.NewGuid();

        var updatingUserId =
            Guid.NewGuid();

        var originalCreatedAtUtc =
            new DateTimeOffset(
                2026,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        var currentUser =
            Substitute.For<ICurrentUserContext>();

        currentUser.UserId
            .Returns(updatingUserId);

        currentUser.ActorType
            .Returns(AuditActorType.User);

        var interceptor =
            new AuditableEntityInterceptor(
                currentUser,
                Microsoft.Extensions.Options.Options.Create(
                    new AuditingOptions()));

        await using var connection =
            new Microsoft.Data.Sqlite.SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(interceptor)
                .Options;

        await using var dbContext =
            new TestDbContext(options);

        await dbContext.Database
            .EnsureCreatedAsync();

        var entity =
            new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Original",
                CreatedByUserId = originalUserId,
                UpdatedByUserId = originalUserId,
                CreatedAtUtc = originalCreatedAtUtc,
                UpdatedAtUtc = originalCreatedAtUtc
            };

        dbContext.Entities.Add(entity);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        currentUser.UserId
            .Returns(updatingUserId);

        var storedEntity =
            await dbContext.Entities
                .SingleAsync(
                    x => x.Id == entity.Id);

        storedEntity.Name =
            "Updated";

        await dbContext.SaveChangesAsync();

        storedEntity.CreatedByUserId.Should()
            .Be(originalUserId);

        storedEntity.CreatedAtUtc.Should()
            .Be(originalCreatedAtUtc);

        storedEntity.UpdatedByUserId.Should()
            .Be(updatingUserId);

        storedEntity.UpdatedAtUtc.Should()
            .BeAfter(originalCreatedAtUtc);
    }
}
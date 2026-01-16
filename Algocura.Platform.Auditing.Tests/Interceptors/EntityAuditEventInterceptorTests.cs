using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Events;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Platform.Auditing.Models;
using Algocura.Platform.Auditing.Options;
using Algocura.Platform.Auditing.Sanitization;
using Algocura.Platform.Auditing.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Algocura.Platform.Auditing.Tests.Interceptors;

public sealed class EntityAuditEventInterceptorTests
{
    [Fact]
    public async Task SaveChanges_NewEntity_PublishesAuditEvent()
    {
        var currentUser =
            Substitute.For<ICurrentUserContext>();

        var userId =
            Guid.NewGuid();

        currentUser.UserId
            .Returns(userId);

        currentUser.Subject
            .Returns(userId.ToString());

        currentUser.ActorType
            .Returns(AuditActorType.User);

        currentUser.DisplayName
            .Returns("Test User");

        currentUser.Email
            .Returns("test@example.com");

        currentUser.CorrelationId
            .Returns("correlation-123");

        currentUser.TraceId
            .Returns("trace-123");

        currentUser.IpAddress
            .Returns("127.0.0.1");

        currentUser.UserAgent
            .Returns("UnitTest");

        var publisher =
            Substitute.For<IAuditEventPublisher>();

        EntityAuditEvent? capturedEvent = null;

        publisher.PublishAsync(
                Arg.Do<EntityAuditEvent>(
                    auditEvent =>
                        capturedEvent = auditEvent),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var options =
            Microsoft.Extensions.Options.Options.Create(
                new AuditingOptions
                {
                    ServiceName =
                        "Algocura.Test.Api"
                });

        var sanitizer =
            new DefaultAuditValueSanitizer(
                options);

        var interceptor =
            new EntityAuditEventInterceptor(
                currentUser,
                publisher,
                sanitizer,
                options);

        await using var connection =
            new Microsoft.Data.Sqlite.SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var dbOptions =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(interceptor)
                .Options;

        await using var dbContext =
            new TestDbContext(dbOptions);

        await dbContext.Database
            .EnsureCreatedAsync();

        var entityId =
            Guid.NewGuid();

        dbContext.Entities.Add(
            new TestEntity
            {
                Id = entityId,
                Name = "Entity",
                Password = "DoNotAudit"
            });

        await dbContext.SaveChangesAsync();

        await publisher
            .Received(1)
            .PublishAsync(
                Arg.Any<EntityAuditEvent>(),
                Arg.Any<CancellationToken>());

        capturedEvent.Should()
            .NotBeNull();

        capturedEvent!.ServiceName.Should()
            .Be("Algocura.Test.Api");

        capturedEvent.EntityType.Should()
            .Be(nameof(TestEntity));

        capturedEvent.EntityId.Should()
            .Be(entityId.ToString());

        capturedEvent.Action.Should()
            .Be(AuditAction.Created);

        capturedEvent.UserId.Should()
            .Be(userId);

        capturedEvent.ActorType.Should()
            .Be(AuditActorType.User);

        capturedEvent.Subject.Should()
            .Be(userId.ToString());

        capturedEvent.UserDisplayName.Should()
            .Be("Test User");

        capturedEvent.CorrelationId.Should()
            .Be("correlation-123");

        capturedEvent.TraceId.Should()
            .Be("trace-123");

        capturedEvent.CurrentValuesJson.Should()
            .NotBeNullOrWhiteSpace();

        capturedEvent.CurrentValuesJson.Should()
            .NotContain("Password");

        capturedEvent.CurrentValuesJson.Should()
            .Contain("Entity");
    }

    [Fact]
    public async Task SaveChanges_ModifiedEntity_PublishesPreviousAndCurrentValues()
    {
        var currentUser =
            Substitute.For<ICurrentUserContext>();

        currentUser.UserId
            .Returns(Guid.NewGuid());

        currentUser.ActorType
            .Returns(AuditActorType.User);

        var publisher =
            Substitute.For<IAuditEventPublisher>();

        var capturedEvents =
            new List<EntityAuditEvent>();

        publisher.PublishAsync(
                Arg.Do<EntityAuditEvent>(
                    auditEvent =>
                        capturedEvents.Add(auditEvent)),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var options =
            Microsoft.Extensions.Options.Options.Create(
                new AuditingOptions
                {
                    ServiceName =
                        "Algocura.Test.Api"
                });

        var sanitizer =
            new DefaultAuditValueSanitizer(
                options);

        var interceptor =
            new EntityAuditEventInterceptor(
                currentUser,
                publisher,
                sanitizer,
                options);

        await using var connection =
            new Microsoft.Data.Sqlite.SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var dbOptions =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(interceptor)
                .Options;

        await using var dbContext =
            new TestDbContext(dbOptions);

        await dbContext.Database
            .EnsureCreatedAsync();

        var entity =
            new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Original"
            };

        dbContext.Entities.Add(entity);

        await dbContext.SaveChangesAsync();

        capturedEvents.Clear();
        publisher.ClearReceivedCalls();

        entity.Name =
            "Updated";

        await dbContext.SaveChangesAsync();

        await publisher
            .Received(1)
            .PublishAsync(
                Arg.Any<EntityAuditEvent>(),
                Arg.Any<CancellationToken>());

        capturedEvents.Should()
            .ContainSingle();

        var capturedEvent =
            capturedEvents.Single();

        capturedEvent.Action.Should()
            .Be(AuditAction.Updated);

        capturedEvent.EntityId.Should()
            .Be(entity.Id.ToString());

        capturedEvent.PreviousValuesJson.Should()
            .NotBeNullOrWhiteSpace();

        capturedEvent.PreviousValuesJson.Should()
            .Contain("Original");

        capturedEvent.CurrentValuesJson.Should()
            .NotBeNullOrWhiteSpace();

        capturedEvent.CurrentValuesJson.Should()
            .Contain("Updated");

        capturedEvent.ChangedPropertiesJson.Should()
            .NotBeNullOrWhiteSpace();

        capturedEvent.ChangedPropertiesJson.Should()
            .Contain("Name");
    }

    [Fact]
    public async Task SaveChanges_DeletedEntity_PublishesDeleteAuditEvent()
    {
        var currentUser =
            Substitute.For<ICurrentUserContext>();

        currentUser.ActorType
            .Returns(AuditActorType.System);

        var publisher =
            Substitute.For<IAuditEventPublisher>();

        var capturedEvents =
            new List<EntityAuditEvent>();

        publisher.PublishAsync(
                Arg.Do<EntityAuditEvent>(
                    auditEvent =>
                        capturedEvents.Add(auditEvent)),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var options =
            Microsoft.Extensions.Options.Options.Create(
                new AuditingOptions
                {
                    ServiceName =
                        "Algocura.Test.Api"
                });

        var sanitizer =
            new DefaultAuditValueSanitizer(
                options);

        var interceptor =
            new EntityAuditEventInterceptor(
                currentUser,
                publisher,
                sanitizer,
                options);

        await using var connection =
            new Microsoft.Data.Sqlite.SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var dbOptions =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(interceptor)
                .Options;

        await using var dbContext =
            new TestDbContext(dbOptions);

        await dbContext.Database
            .EnsureCreatedAsync();

        var entity =
            new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Delete Me"
            };

        dbContext.Entities.Add(entity);

        await dbContext.SaveChangesAsync();

        capturedEvents.Clear();
        publisher.ClearReceivedCalls();

        dbContext.Entities.Remove(entity);

        await dbContext.SaveChangesAsync();

        await publisher
            .Received(1)
            .PublishAsync(
                Arg.Any<EntityAuditEvent>(),
                Arg.Any<CancellationToken>());

        capturedEvents.Should()
            .ContainSingle();

        var capturedEvent =
            capturedEvents.Single();

        capturedEvent.Action.Should()
            .Be(AuditAction.Deleted);

        capturedEvent.EntityId.Should()
            .Be(entity.Id.ToString());

        capturedEvent.ActorType.Should()
            .Be(AuditActorType.System);

        capturedEvent.PreviousValuesJson.Should()
            .NotBeNullOrWhiteSpace();

        capturedEvent.PreviousValuesJson.Should()
            .Contain("Delete Me");
    }
}
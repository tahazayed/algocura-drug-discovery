using Algocura.Notification.Api.Contracts;
using Algocura.Notification.Api.Controllers;
using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Algocura.Notification.Api.Tests.Controllers;

public sealed class NotificationRulesControllerTests
{
    [Fact]
    public async Task Create_ValidRequest_CreatesRule()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var controller =
            new NotificationRulesController(database.Context);

        var action = await controller.Create(
            new CreateNotificationRuleRequest(
                "Rule",
                "TargetSelected",
                NotificationChannel.InApplication,
                "team",
                "Target selected",
                "A target was selected.",
                NotificationPriority.Normal),
            timeout.Token);

        var created = action.Result.Should()
            .BeOfType<CreatedAtActionResult>().Subject;

        var response = created.Value.Should()
            .BeOfType<NotificationRuleResponse>().Subject;

        response.EventType.Should().Be("TargetSelected");
        response.Enabled.Should().BeTrue();
    }
}

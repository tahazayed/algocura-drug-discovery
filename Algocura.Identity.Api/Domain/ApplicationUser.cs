using Algocura.Platform.Auditing.Abstractions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Algocura.Identity.Api.Domain;

public sealed class ApplicationUser
    : IdentityUser<Guid>,
      IAuditableEntity
{
    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

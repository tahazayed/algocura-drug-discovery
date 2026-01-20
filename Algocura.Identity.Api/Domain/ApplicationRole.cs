using Algocura.Platform.Auditing.Abstractions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Algocura.Identity.Api.Domain;

public sealed class ApplicationRole
    : IdentityRole<Guid>,
      IAuditableEntity
{
    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Identity;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Platform.Auditing.Options;
using Algocura.Platform.Auditing.Publishing;
using Algocura.Platform.Auditing.Sanitization;
using Microsoft.Extensions.DependencyInjection;

namespace Algocura.Platform.Auditing.DependencyInjection;

public static class AuditingServiceCollectionExtensions
{
    public static IServiceCollection AddAlgocuraAuditing(
        this IServiceCollection services,
        Action<AuditingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        ArgumentNullException.ThrowIfNull(
            configure);

        services.AddHttpContextAccessor();

        services.Configure(
            configure);

        services.AddScoped<
            ICurrentUserContext,
            CurrentUserContext>();

        services.AddScoped<
            IAuditValueSanitizer,
            DefaultAuditValueSanitizer>();

        services.AddScoped<
            AuditableEntityInterceptor>();

        services.AddScoped<
            EntityAuditEventInterceptor>();

        services.AddScoped<
            IAuditEventPublisher,
            NullAuditEventPublisher>();

        return services;
    }

    public static IServiceCollection AddAlgocuraAuditing<
        TAuditEventPublisher>(
        this IServiceCollection services,
        Action<AuditingOptions> configure)
        where TAuditEventPublisher :
            class,
            IAuditEventPublisher
    {
        services.AddAlgocuraAuditing(
            configure);

        services.AddScoped<
            IAuditEventPublisher,
            TAuditEventPublisher>();

        return services;
    }
}

using Algocura.Platform.Security.Authorization;
using Algocura.Platform.Security.Context;
using Algocura.Platform.Security.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Algocura.Platform.Security.DependencyInjection;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddAlgocuraPlatformSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        ArgumentNullException.ThrowIfNull(
            configuration);

        services
            .AddOptions<JwtOptions>()
            .Bind(
                configuration.GetSection(
                    JwtOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Issuer),
                "Jwt:Issuer is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Audience),
                "Jwt:Audience is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.SigningKey) &&
                    options.SigningKey.Length >= 32,
                "Jwt:SigningKey must contain at least 32 characters.")
            .Validate(
                options =>
                    options.AccessTokenMinutes > 0,
                "Jwt:AccessTokenMinutes must be greater than zero.")
            .Validate(
                options =>
                    options.ClockSkewMinutes >= 0,
                "Jwt:ClockSkewMinutes cannot be negative.")
            .ValidateOnStart();

        services
            .AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
            .AddJwtBearer(
                options =>
                {
                    var jwtOptions =
                        configuration
                            .GetSection(
                                JwtOptions.SectionName)
                            .Get<JwtOptions>()
                        ?? throw new InvalidOperationException(
                            "Jwt configuration is required.");

                    options.MapInboundClaims =
                        true;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer =
                                true,

                            ValidIssuer =
                                jwtOptions.Issuer,

                            ValidateAudience =
                                true,

                            ValidAudience =
                                jwtOptions.Audience,

                            ValidateIssuerSigningKey =
                                true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtOptions.SigningKey)),

                            ValidateLifetime =
                                true,

                            ClockSkew =
                                TimeSpan.FromMinutes(
                                    jwtOptions.ClockSkewMinutes),

                            NameClaimType =
                                ClaimTypes.Name,

                            RoleClaimType =
                                ClaimTypes.Role
                        };
                });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                PlatformPolicies.AuthenticatedUser,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                })
            .AddPolicy(
                PlatformPolicies.AdministratorOnly,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireRole(
                        PlatformRoles.Administrator);
                })
            .AddPolicy(
                PlatformPolicies.ScientistOrAdministrator,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireRole(
                        PlatformRoles.Administrator,
                        PlatformRoles.Scientist);
                })
            .AddPolicy(
                PlatformPolicies.ReviewerOrAdministrator,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireRole(
                        PlatformRoles.Administrator,
                        PlatformRoles.Reviewer);
                })
            .AddPolicy(
                PlatformPolicies.ServiceOrAdministrator,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireRole(
                        PlatformRoles.Administrator,
                        PlatformRoles.Service);
                });

        services.AddHttpContextAccessor();

        services.AddScoped<
            ICurrentUserContext,
            HttpCurrentUserContext>();

        return services;
    }

    public static IServiceCollection AddAlgocuraPlatformSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<Microsoft.AspNetCore.Authorization.AuthorizationOptions>
            configureAuthorization)
    {
        ArgumentNullException.ThrowIfNull(
            configureAuthorization);

        services.AddAlgocuraPlatformSecurity(
            configuration);

        services.Configure(
            configureAuthorization);

        return services;
    }
}

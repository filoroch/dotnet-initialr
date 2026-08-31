using Filoroch.Template.Application.Autenticacao.Services;
using Filoroch.Template.IoC.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Filoroch.Template.IoC.Services;
using Filoroch.Template.Domain.Usuarios.Services;

namespace Filoroch.Template.IoC.Configurations;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddProjectAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtSettings settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("A configuração Jwt é obrigatória.");

        if (string.IsNullOrWhiteSpace(settings.Issuer) || string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("Jwt:Issuer e Jwt:Audience são obrigatórios.");

        if (string.IsNullOrWhiteSpace(settings.SigningKey) || settings.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey deve possuir pelo menos 32 caracteres.");

        if (settings.AccessTokenMinutes <= 0)
            throw new InvalidOperationException("Jwt:AccessTokenMinutes deve ser maior que zero.");

        if (settings.ClockSkewSeconds < 0)
            throw new InvalidOperationException("Jwt:ClockSkewSeconds não pode ser negativo.");

        services.AddSingleton(settings);
        services.AddScoped<IPasswordService, BCryptPasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey))
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

        return services;
    }
}

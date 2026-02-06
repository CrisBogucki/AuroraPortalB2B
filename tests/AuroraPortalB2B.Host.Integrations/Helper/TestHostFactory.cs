using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Host.Integrations.Helper;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestHostFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes("Test")
                    .RequireAuthenticatedUser()
                    .Build();
            });

            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<PartnersDbContext>))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            var npgsqlDescriptors = services
                .Where(d =>
                    d.ImplementationType?.Namespace?.StartsWith("Npgsql.EntityFrameworkCore.PostgreSQL") == true
                    || d.ServiceType?.Namespace?.StartsWith("Npgsql.EntityFrameworkCore.PostgreSQL") == true)
                .ToList();

            foreach (var descriptor in npgsqlDescriptors)
            {
                services.Remove(descriptor);
            }

            var inMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<PartnersDbContext>(options =>
                options.UseInMemoryDatabase("partners_test")
                    .UseInternalServiceProvider(inMemoryProvider));

        });
    }
}

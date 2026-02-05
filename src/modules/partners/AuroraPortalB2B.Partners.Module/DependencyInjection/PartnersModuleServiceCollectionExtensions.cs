using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Extensions;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Abstractions.System;
using AuroraPortalB2B.Partners.Endpoints.Dtos;
using AuroraPortalB2B.Partners.Endpoints.Validators;
using AuroraPortalB2B.Partners.Infrastructure.DependencyInjection;
using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using AuroraPortalB2B.Partners.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Partners.Module.DependencyInjection;

public static class PartnersModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPartnersModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PartnersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IPartnerUserRepository, PartnerUserRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();

        services.AddMediator(options =>
        {
            options.AddAssemblies(typeof(CreatePartnerCommand).Assembly);
            options.AddPipelineBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        services.AddScoped<IValidator<CreatePartnerRequest>, CreatePartnerRequestValidator>();
        services.AddScoped<IValidator<CreatePartnerUserRequest>, CreatePartnerUserRequestValidator>();
        services.AddScoped<IValidator<UpdatePartnerRequest>, UpdatePartnerRequestValidator>();
        services.AddScoped<IValidator<UpdatePartnerUserRequest>, UpdatePartnerUserRequestValidator>();

        return services;
    }
}

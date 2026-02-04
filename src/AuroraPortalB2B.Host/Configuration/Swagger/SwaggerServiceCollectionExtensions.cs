namespace AuroraPortalB2B.Host.Configuration.Swagger;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddHostSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<BearerSecurityOperationFilter>();
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        return services;
    }
}

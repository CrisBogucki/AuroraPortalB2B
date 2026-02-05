using AuroraPortalB2B.Host.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHostServices(builder.Configuration);

var app = builder.Build();

app.UseHostPipeline();
app.MapHostEndpoints();

await app.RunAsync();

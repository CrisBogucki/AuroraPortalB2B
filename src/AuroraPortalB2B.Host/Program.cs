using AuroraPortalB2B.Host.Configuration;
using AuroraPortalB2B.Host.Configuration.Endpoints;
using AuroraPortalB2B.Host.Configuration.Pipeline;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext();

if (builder.Environment.IsDevelopment())
{
    loggerConfig = loggerConfig.WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (trace={TraceId} user={Username}){NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code);
}
else
{
    loggerConfig = loggerConfig.WriteTo.Console(new RenderedCompactJsonFormatter());
}

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHostServices(builder.Configuration);

var app = builder.Build();

app.UseHostPipeline();
app.MapHostEndpoints();

await app.RunAsync();

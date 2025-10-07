using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Middlewares;
using Device.FingerPrint.Gateway.Models;
using Device.FingerPrint.Gateway.Services;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<IAppSettings, AppSettings>();
builder.Services.AddSingleton<IGatewaySettings, GatewaySettings>();
builder.Services.AddSingleton<ILogger, Logger>();

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder => builder
                    .WithOrigins("*")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(30)));
});


builder.Services.AddSwaggerGen();

builder.Services
    .AddOcelot(builder.Configuration)
    .AddDelegatingHandler<IPCaptureHandler>()
    .AddDelegatingHandler<MobileRequestHandler>();

var app = builder.Build();

var appSettings = app.Services.GetRequiredService<IAppSettings>();
var gatewaySettings = app.Services.GetRequiredService<IGatewaySettings>();

LogManager
    .SetDefaultLogCategory("FPS-Gateway-API")
    .Use(new List<ILogEngine>
    {
            new SentryLogging(new SentryEngineOptions
            {
                Dsn = !string.IsNullOrEmpty(appSettings.SentryDsn) ? appSettings.SentryDsn : null,
                Environment = appSettings.Environment ?? "development",
                Release = builder.Configuration["Sentry:Release"]
            }),
    });

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors();

app.UseEndpoints(endpoints =>
    endpoints.MapControllers()
);

// Add Prometheus metrics
app.UseMetricServer();
app.UseHttpMetrics();
MetricHttpService.PublicDataToGateway(gatewaySettings.GatewayInfo);

app.UseHealthChecks("/healthz");

await app.UseOcelot();

LogService.Info($"This is Gateway API {typeof(Program).FullName} has been started at {DateTimeOffset.Now}");

app.Run();
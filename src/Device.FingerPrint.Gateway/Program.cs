using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Middlewares;
using Device.FingerPrint.Gateway.Models;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Gateway.Configuration;
using Device.FingerPrint.Gateway.RateLimit;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Ocelot.RateLimiting;
using CacheManager.Core;
using CacheManager.Redis;
using CacheManager.Microsoft.Extensions.Caching.Memory;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure Redis settings
builder.Services.Configure<RedisConfiguration>(
    builder.Configuration.GetSection("Redis"));

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

// Configure Redis connection string
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ??
                           builder.Configuration["Redis:ConnectionString"] ??
                           "localhost:6379";

// Register custom rate limit services
builder.Services.AddSingleton<IClientIdService, IPAddressClientIdentifier>();
builder.Services.AddSingleton<IClientWhitelistService, ClientIdentityProvider>();

// Configure CacheManager with Redis for Ocelot
builder.Services.AddSingleton<ICacheManager>(x =>
{
    var configuration = ConfigurationBuilder.New()
        .WithUpdateMode(CacheUpdateMode.Up)
        .WithRedis(redisConnectionString)
        .WithMicrosoftCaching() // Fallback cache
        .WithRedisConfiguration("myRedis", config =>
        {
            config.WithAllowAdmin()
                  .WithDatabase(0)
                  .WithConnectionString(redisConnectionString);
        })
        .WithRedisCaching("myRedis", true)
        .WithMicrosoftMemoryCaching()
        .WithRedisKeyPrefix("ocelot_rate_limit:")
        .And
        .WithSerializer(typeof(CacheManager.Serialization.Json.JsonCacheSerializer));
    return new BaseCacheManager(configuration);
});

builder.Services
    .AddOcelot(builder.Configuration)
    .AddCacheManager()
    .AddDelegatingHandler<IPCaptureHandler>();

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
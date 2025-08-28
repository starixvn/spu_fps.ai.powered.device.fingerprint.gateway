using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Gateway.Middleware;
using Device.FingerPrint.Redis;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Sentry;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.MsgPack;
using ILogger = Fanex.Logging.ILogger;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();

// Add memory cache
builder.Services.AddMemoryCache();

// Add HttpContextAccessor for IPCaptureHandler
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services
    .AddOcelot(builder.Configuration)
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    })
    .AddDelegatingHandler<ValidateHandler>()
    .AddDelegatingHandler<IPCaptureHandler>()
    .AddDelegatingHandler<MobileRequestHandler>();

var appSettings = new Device.FingerPrint.Gateway.AppSettings(builder.Configuration);

var gatewaySetting = new GatewaySetting(builder.Configuration);
builder.Services.AddSingleton<IGatewaySetting>(gatewaySetting);

builder.Services.AddSingleton<Device.FingerPrint.Gateway.IAppSettings>(appSettings);
builder.Services.AddSingleton<ILogger, Logger>();

var redisConfiguration = builder.Configuration.GetSection("Redis").Get<RedisConfiguration>();

// Cải thiện cấu hình Redis để tăng tính ổn định
if (redisConfiguration != null)
{
    // Tăng timeout và retry settings
    redisConfiguration.ConnectTimeout = 10000;
    redisConfiguration.ConnectRetry = 10;
    redisConfiguration.AbortOnConnectFail = false;

    // Cấu hình connection pool
    redisConfiguration.PoolSize = 50;
}

builder.Services.AddSingleton(redisConfiguration);

// Cấu hình Redis với retry policy và circuit breaker
builder.Services.AddSingleton<IRedisClient, RedisClient>();
builder.Services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();
builder.Services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddSingleton<ISubscriberService, SubscriberService>();
builder.Services.AddSingleton<IGatewayService, GatewayService>();

LogManager
          .SetDefaultLogCategory("FPS-Gateway-API")
          .Use(new List<ILogEngine>
          {
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = !string.IsNullOrEmpty(appSettings.SentryDsn) ? new Dsn(appSettings.SentryDsn) : null,
                        Environment = appSettings.Environment ?? "development",
                        Release = builder.Configuration["Sentry:Release"]
                    }),
          });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RedisHealthMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Map controllers
});

// Add Prometheus metrics
app.UseMetricServer();
app.UseHttpMetrics();

MetricHttpService.PublicDataToGateway(gatewaySetting.GatewayInfo);

app.UseHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true
});

// Ocelot middleware phải được đặt sau routing và trước endpoints
await app.UseOcelot();

LogService.Info($"This is Gateway API {typeof(Program).FullName} has been started at {DateTimeOffset.Now}");

app.Run();

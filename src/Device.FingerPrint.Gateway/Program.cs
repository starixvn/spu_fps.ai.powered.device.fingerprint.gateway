using Device.FingerPrint.Gateway;
using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Middlewares;
using Device.FingerPrint.Gateway.Services;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.MsgPack;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<IAppSettings, AppSettings>();
builder.Services.AddSingleton<IGatewaySettings, GatewaySettings>();
builder.Services.AddSingleton<ILogger, Logger>();

var redisConfiguration = builder.Configuration.GetSection("Redis").Get<RedisConfiguration>();
if (redisConfiguration != null)
{
    redisConfiguration.ConnectTimeout = 10000;
    redisConfiguration.ConnectRetry = 10;
    redisConfiguration.AbortOnConnectFail = false;

    redisConfiguration.PoolSize = 50;
}

builder.Services.AddSingleton(redisConfiguration);

builder.Services.AddSingleton<IRedisClient, RedisClient>();
builder.Services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();
builder.Services.AddSingleton<ISerializer, MsgPackObjectSerializer>();

builder.Services.AddSingleton<ISubscriberService, SubscriberService>();
builder.Services.AddSingleton<IRedisService, RedisService>();

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

builder.Services.AddMemoryCache();

builder.Services.AddSwaggerGen();

builder.Services
    .AddOcelot(builder.Configuration)
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    })
    .AddDelegatingHandler<ValidateHandler>()
    .AddDelegatingHandler<IPCaptureHandler>()
    .AddDelegatingHandler<MobileRequestHandler>()
    .AddDelegatingHandler<RequestCounterHandler>();

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
app.UseMiddleware<RedisHealthMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

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
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FPS AI Powered Device Fingerprint Gateway API",
        Version = "v1",
        Description = "API Gateway for Device Fingerprint Management with AI capabilities",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "FPS Team",
            Email = "support@fps.com"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Enable Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FPS AI Powered Device Fingerprint Gateway API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "FPS AI Powered Device Fingerprint Gateway API";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

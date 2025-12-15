using MedicalCenter.Infrastructure;
using MedicalCenter.WebApi.Middleware;
using FastEndpoints;
using FastEndpoints.Swagger;
using MedicalCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddInfrastructure(builder.Configuration);

// Add FastEndpoints with Swagger support
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.DocumentName = "v1";
            s.Title = "Medical Center API";
            s.Version = "v1";
            s.Description = "Medical Center Automation System API";
        };
    });

// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

// Apply database migrations automatically (for Docker/containerized environments)
try
{
    using IServiceScope scope = app.Services.CreateScope();
    MedicalCenterDbContext dbContext = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
    dbContext.Database.Migrate();
}
catch (Exception ex)
{
    // Log migration errors but don't fail startup
    // In production, migrations should be handled separately
    ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying database migrations");
}

// Configure the HTTP request pipeline
app.UseHttpsRedirection();

// Use exception handler early to catch exceptions from all subsequent middleware
app.UseExceptionHandler();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Use FastEndpoints (this maps the endpoints) and Swagger
app.UseFastEndpoints(c => {
    c.Endpoints.RoutePrefix = "api";
    c.Errors.UseProblemDetails();
}).UseSwaggerGen();

app.Run();

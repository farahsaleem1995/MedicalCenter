using MedicalCenter.Infrastructure;
using MedicalCenter.WebApi.Middleware;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();

// Use global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Use FastEndpoints (this maps the endpoints) and Swagger
app.UseFastEndpoints()
    .UseSwaggerGen();

app.Run();

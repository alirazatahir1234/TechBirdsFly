using TemplateService.Application.Commands;
using TemplateService.Application.DTOs;
using TemplateService.Application.Extensions;
using TemplateService.Application.Queries;
using TemplateService.Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TemplateService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Cloud Run port
var port = Environment.GetEnvironmentVariable("PORT") ?? "5011";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
    {
        p.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

var mediator = app.Services.GetRequiredService<IMediator>();

/// <summary>
/// POST /api/templates - Create a new template
/// </summary>
app.MapPost("/api/templates", async (CreateTemplateRequest request) =>
{
    var command = new CreateTemplateCommand(request);
    var result = await mediator.Send(command);
    return Results.Created($"/api/templates/{result.Id}", result);
})
.WithName("CreateTemplate")
.WithOpenApi();

/// <summary>
/// GET /api/templates - List all templates with optional filtering
/// </summary>
app.MapGet("/api/templates", async (string? category, string? search) =>
{
    var query = new GetTemplatesQuery(category, search);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("ListTemplates")
.WithOpenApi();

/// <summary>
/// GET /api/templates/{id} - Get template by ID
/// </summary>
app.MapGet("/api/templates/{id:guid}", async (Guid id) =>
{
    var query = new GetTemplateByIdQuery(id);
    var result = await mediator.Send(query);
    return result == null ? Results.NotFound() : Results.Ok(result);
})
.WithName("GetTemplateById")
.WithOpenApi();

/// <summary>
/// POST /api/templates/{id}/preview - Upload preview image
/// </summary>
app.MapPost("/api/templates/{id:guid}/preview", async (Guid id, IFormFile file) =>
{
    if (file.Length == 0)
        return Results.BadRequest("File is empty");

    using var stream = file.OpenReadStream();
    var command = new UploadPreviewImageCommand(id, stream, file.FileName);
    var result = await mediator.Send(command);

    return Results.Ok(new { previewUrl = result });
})
.WithName("UploadPreviewImage")
.WithOpenApi()
.DisableAntiforgery();

/// <summary>
/// POST /api/templates/{id}/files - Upload template files
/// </summary>
app.MapPost("/api/templates/{id:guid}/files", async (Guid id, Dictionary<string, string> files) =>
{
    if (files.Count == 0)
        return Results.BadRequest("No files provided");

    var command = new UploadTemplateFilesCommand(id, files);
    await mediator.Send(command);

    return Results.Ok(new { success = true, message = "Files uploaded successfully" });
})
.WithName("UploadTemplateFiles")
.WithOpenApi();

/// <summary>
/// GET /api/templates/health - Health check endpoint
/// </summary>
app.MapGet("/api/templates/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
})
.WithName("HealthCheck")
.WithOpenApi();

app.Run();

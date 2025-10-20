using Microsoft.EntityFrameworkCore;
using GeneratorService.Data;
using GeneratorService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core SQLite
builder.Services.AddDbContext<GeneratorDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("GeneratorDb") ?? "Data Source=generator.db"));

// Services (no RabbitMQ for MVP local dev)
builder.Services.AddScoped<IGeneratorService, GeneratorService.Services.GeneratorService>();
builder.Services.AddScoped<IMessagePublisher, LocalMessagePublisher>();

var app = builder.Build();

// Migrate DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GeneratorDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

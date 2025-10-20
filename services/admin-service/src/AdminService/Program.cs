using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AdminService.Data;
using AdminService.Models;
using AdminService.Services;
using AdminService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR for real-time communication
builder.Services.AddSignalR();

// EF Core SQLite
builder.Services.AddDbContext<AdminDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("AdminDb") ?? "Data Source=admin.db");
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-change-in-production";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TechBirdsFly";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

// Services
builder.Services.AddScoped<IAdminService, global::AdminService.Services.AdminService>();
builder.Services.AddScoped<IUserManagementService, global::AdminService.Services.UserManagementService>();
builder.Services.AddScoped<IRoleManagementService, global::AdminService.Services.RoleManagementService>();
builder.Services.AddScoped<IAnalyticsService, global::AdminService.Services.AnalyticsService>();
builder.Services.AddScoped<IRealTimeService, global::AdminService.Services.RealTimeService>();

var app = builder.Build();

// Migrate DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<AdminHub>("/hubs/admin");

app.Run();

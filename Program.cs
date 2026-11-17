using CourseManagement.Data;
using CourseManagement.Repositories;
using CourseManagement.Repositories.Interfaces;
using CourseManagement.Services;
using CourseManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string not found."
    );
}

builder.Services.AddDbContext<CourseDbContext>(
    options => options.UseMySQL(connectionString)
);

// Repositories
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

// JWT configuration
string? accessSecret = builder.Configuration["JWT_ACCESS_SECRET"];

if (string.IsNullOrWhiteSpace(accessSecret))
{
    throw new InvalidOperationException(
        "JWT_ACCESS_SECRET is missing."
    );
}

byte[] accessKey = Convert.FromBase64String(accessSecret);

if (accessKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT_ACCESS_SECRET must contain at least 32 bytes."
    );
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "CourseManagementAPI",

            ValidateAudience = true,
            ValidAudience = "CourseManagementClient",

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(accessKey),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,

            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Read access token from the cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token =
                    context.Request.Cookies["accessToken"];

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Course Management API"
        );
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
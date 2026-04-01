using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// ==========================
// DATABASE INIT + RETRY
// ==========================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    var retries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int i = 1; i <= retries; i++)
    {
        try
        {
            logger.LogInformation("Connexion à la base de données... tentative {Attempt}/{MaxAttempts}", i, retries);

            context.Database.Migrate();

            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Id = 1,
                        FirstName = "Jean",
                        LastName = "Dupont",
                        Email = "jean@test.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
                    },
                    new User
                    {
                        Id = 2,
                        FirstName = "Marie",
                        LastName = "Durand",
                        Email = "marie@test.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
                    },
                    new User
                    {
                        Id = 3,
                        FirstName = "Paul",
                        LastName = "Martin",
                        Email = "paul@test.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
                    },
                    new User
                    {
                        Id = 4,
                        FirstName = "Lucas",
                        LastName = "Bernard",
                        Email = "lucas@test.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
                    },
                    new User
                    {
                        Id = 5,
                        FirstName = "Emma",
                        LastName = "Petit",
                        Email = "emma@test.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
                    }
                );

                context.SaveChanges();
            }

            logger.LogInformation("Base de données prête.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Impossible de se connecter à la base.");

            if (i == retries)
                throw;

            Thread.Sleep(delay);
        }
    }
}

app.Run();
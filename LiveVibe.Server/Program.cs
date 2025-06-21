using System.Security.Claims;
using System.Text;
using LiveVibe.Server.HelperClasses;
using LiveVibe.Server.Models.Tables;
using LiveVibe.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Configure Database
        var connection = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var maxRetryCount = builder.Configuration.GetValue("Database:MaxRetryCount", 10);
        var maxRetryDelay = builder.Configuration.GetValue("Database:MaxRetryDelay", 10);
            
        builder.Services.AddDbContext<ApplicationContext>(options =>
            options.UseSqlServer(connection,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                        errorNumbersToAdd: null);
                }));

        // 2. Configure Identity
        builder.Services
            .AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders();

        // 3. Configure JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        // 4. Authorization
        builder.Services.AddAuthorization();

        // 5. Register custom services
        builder.Services.AddScoped<IQRCodeService, QRCodeService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        // 6. Controllers and Swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter 'Bearer' [space] and then your valid JWT token.\r\nExample: Bearer abcdef12345",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "bearer",
                        Name = "Authorization",
                        In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddResponseCaching();
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        builder.Services.AddCors(options => {
            options.AddPolicy("DefaultPolicy",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        builder.Services.AddHealthChecks();

        // 7. Configure the port explicitly
        builder.WebHost.UseUrls("http://0.0.0.0:5000");

        var app = builder.Build();

        // 8. Fully setup the db
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationContext>();

            await context.Database.MigrateAsync();

            await SeedData.InitializeAsync(services);
        }

        // 9. Middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => options.SerializeAsV2 = true);
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();

        app.UseCors("DefaultPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            await next();
        });

        app.UseResponseCaching();
        app.UseResponseCompression();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
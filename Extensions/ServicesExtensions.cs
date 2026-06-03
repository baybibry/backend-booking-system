using System.Text;
using System.Threading.RateLimiting;
using BookingSystem.Concurrency;
using BookingSystem.Data;
using BookingSystem.Helper;
using BookingSystem.Enums;
using BookingSystem.Wrapper;
using Microsoft.AspNetCore.Mvc;
using BookingSystem.Repositories.Implementations;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Implementations;
using BookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace BookingSystem.Extensions;

public static class ServicesExtensions
{
    public static void CommonServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullConverter())
            )
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            e => e.Key,
                            e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                        );

                    var wrapper = ResponseWrapper<Dictionary<string, string[]>>.On(
                        errors,
                        "Validation failed.",
                        StatusType.Error,
                        StatusCodes.Status400BadRequest
                    );

                    return new BadRequestObjectResult(wrapper);
                };
            });
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Booking System API",
                Version = "v1",
                Description = "REST API for managing appointments between patients and doctors."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token. Example: eyJhbGci..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IScheduleLock, ScheduleLock>();
        services.AddSingleton<IAppointmentLock, AppointmentLock>();

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ISpecializationRepository, SpecializationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReceptionistRepository, ReceptionistRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<ISpecializationService, SpecializationService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReceptionistService, ReceptionistService>();
    }

    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
                policy.SetIsOriginAllowed(origin =>
                {
                    if (origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                        return true;

                    // Use for testing local network accessing Backend in different device
                    if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    {
                        var host = uri.Host;
                        if (host == "localhost" || host == "127.0.0.1") return true;
                        if (host.StartsWith("192.168.")) return true;
                        if (host.StartsWith("10.")) return true;
                    }

                    return false;
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            );
        });
    }

    public static void AddRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        var permitLimit = config.GetValue<int>("RateLimit:PermitLimit", 100);
        var windowSeconds = config.GetValue<int>("RateLimit:WindowSeconds", 60);
        var queueLimit = config.GetValue<int>("RateLimit:QueueLimit", 0);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("fixed", ctx =>
            {
                var key = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? ctx.Connection.RemoteIpAddress?.ToString()
                          ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    QueueLimit = queueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType = "application/json";
                await ctx.HttpContext.Response.WriteAsync(
                    "{\"message\":\"Too many requests. Please slow down.\",\"type\":\"Error\",\"statusCode\":429}",
                    ct
                );
            };
        });
    }

    public static void AddDbContextExtension(
        this IServiceCollection services,
        string connectionString
    ) =>
        services.AddDbContext<BookingContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null
                )
            )
        );

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var key = config["JWT:KEY"];
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            throw new InvalidOperationException("JWT:KEY must be at least 32 characters long.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JWT:ISSUER"],
                    ValidAudience = config["JWT:AUDIENCE"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:KEY"]!))
                };
            });
    }
}

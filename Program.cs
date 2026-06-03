using BookingSystem.Extensions;
using BookingSystem.Scheduler;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Host.UseSerilog((ctx, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
);

builder.Services.AddServices();
builder.Services.CommonServices();
builder.Services.AddRepositories();
builder.Services.AddDbContextExtension(dbConnection);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHostedService<AppointmentStatusScheduler>();
builder.Services.AddSwagger();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);

var app = builder.Build();

app.UseCommonConfiguration();

app.Run();
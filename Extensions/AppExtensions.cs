using BookingSystem.Middleware;
using Serilog;

namespace BookingSystem.Extensions;

public static class AppExtensions
{
    public static void UseCommonConfiguration(this WebApplication app)
    {
        app.UseCors("DefaultCors");

        app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking System API v1");
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
            });
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
            app.UseHsts();
        }
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireRateLimiting("fixed");
    }
}

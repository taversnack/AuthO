namespace STSL.SmartLocker.Utils.Redirect.WebApp
{
    public class Program
    {
        const string lockerSupportUrl = "https://www.uclhlocker.support/";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();

            var app = builder.Build();

            app.MapGet("/a/{lockerId}", (string lockerId, HttpContext context, ILogger<Program> logger) =>
            {
                LockerSupportRedirect(context, logger);
            });

            app.MapGet("/n/{lockerLocation}", (string lockerLocation, HttpContext context, ILogger<Program> logger) =>
            {
                LockerSupportRedirect(context, logger);
            });

            app.MapGet("/", () => string.Empty);

            app.Run();
        }

        private static void LockerSupportRedirect(HttpContext context, ILogger<Program> logger)
        {
            logger.LogInformation("Redirecting {path} to {redirect} for request from {ipAddress}", context.Request.Path, lockerSupportUrl, context.Connection.RemoteIpAddress);
            context.Response.Redirect(lockerSupportUrl, permanent: false);
        }
    }
}
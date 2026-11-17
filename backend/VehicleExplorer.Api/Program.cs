using System.Threading.RateLimiting;
using VehicleExplorer.Api.ErrorHandling;
using VehicleExplorer.Api.RateLimiting;
using VehicleExplorer.Application.Services;
using VehicleExplorer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks();

// The makes list is over 12,000 items, so compressing JSON responses pays off.
builder.Services.AddResponseCompression();


var rateLimiting = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
    ?? new RateLimitingOptions();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Written explicitly because the default ProblemDetails has no title for 429.
    options.OnRejected = async (context, cancellationToken) =>
    {
        var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context.HttpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = $"Limit is {rateLimiting.PermitLimit} requests per {rateLimiting.WindowSeconds} seconds.",
            },
        });
    };

    options.AddPolicy(RateLimitingOptions.PerIpPolicyName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimiting.PermitLimit,
                Window = TimeSpan.FromSeconds(rateLimiting.WindowSeconds),
                QueueLimit = 0,
            }));
});

var app = builder.Build();

app.UseExceptionHandler();

// Gives error responses without a body (such as 404) the same ProblemDetails shape as exceptions.
app.UseStatusCodePages();

app.UseResponseCompression();

// Serves the Angular build when it is copied into wwwroot, so frontend and API share one origin and need no CORS.
app.UseStaticFiles();

app.UseRateLimiter();

app.MapControllers()
    .RequireRateLimiting(RateLimitingOptions.PerIpPolicyName);
app.MapHealthChecks("/health");

app.MapFallback("/api/{**path}", () => Results.NotFound());

// Any other unknown URL is an Angular route, so the browser gets index.html and Angular takes over.
app.MapFallbackToFile("index.html");

app.Run();

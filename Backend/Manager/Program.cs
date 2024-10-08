using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

using Sharp.Backend.Manager;

using Options = Sharp.Backend.Manager.Options;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var optionsBuilder = services
    .AddOptions<Options>()
    .BindConfiguration(nameof(Options))
    .ValidateDataAnnotations();

services
    .AddSingleton<ISandboxProvider, JailSandboxProvider>();

var options = Unsafe.As<Options>(RuntimeHelpers.GetUninitializedObject(typeof(Options)));
builder.Configuration.Bind(nameof(Options), options);

var concurrencyPolicyName = "execute-concurrency";

services
    .AddRateLimiter(o =>
    {
        o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        o.AddConcurrencyLimiter(concurrencyPolicyName, o =>
        {
            o.PermitLimit = options.RateLimits.ConcurrencyLimit;
        });
    });

var app = builder.Build();

app.UseRateLimiter();

app.MapGet("/platforms", (IOptions<Options> options) => options.Value.Platforms);

app.MapPost("/{platform}/run", (Architecture platform, HttpContext context, ISandboxProvider sandboxProvider, IOptions<Options> options) =>
{
    if (!options.Value.Platforms.Contains(platform))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.Body.WriteAsync("The specified platform is not supported."u8.ToArray()).AsTask();
    }

    return sandboxProvider.ExecuteAsync(ContainerFunction.Run, context.Request.Body, context.Response.Body);
}).RequireRateLimiting(concurrencyPolicyName);

app.MapPost("/{platform}/asm", (Architecture platform, HttpContext context, ISandboxProvider sandboxProvider, IOptions<Options> options) =>
{
    if (!options.Value.Platforms.Contains(platform))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.Body.WriteAsync("The specified platform is not supported."u8.ToArray()).AsTask();
    }

    return sandboxProvider.ExecuteAsync(ContainerFunction.Assembly, context.Request.Body, context.Response.Body);
}).RequireRateLimiting(concurrencyPolicyName);

app.Run();

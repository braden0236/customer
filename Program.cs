using customer.Database;
using customer.Endpoints;
using customer.Repositories;
using customer.Services;
using Microsoft.EntityFrameworkCore;
using Monitoring;
using OpenTelemetry.Metrics;
using OpenTelemetry.ResourceDetectors.Container;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddDbContextPool<CustomerContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("CustomerDB"));
    // options.EnableSensitiveDataLogging();
},
    poolSize: 1024
);

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddSingleton<MetricInstrumentation>();

Action<ResourceBuilder> configureResource = resource => resource
    .AddService(
        serviceName: builder.Configuration.GetValue("ServiceName", defaultValue: "otel-test0")!,
        serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
        serviceInstanceId: Environment.MachineName
    )
    .AddDetector(new ContainerResourceDetector());

builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithMetrics(meterBuilder => meterBuilder
        .AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter()
        .AddMeter(MetricInstrumentation.MeterName))
    .WithTracing(tracerBuilder => tracerBuilder
        .SetSampler(new AlwaysOnSampler())
        .AddRedisInstrumentation(options =>
        {
            options.SetVerboseDatabaseStatements = true;
        })
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = httpContext =>
            {
                if (httpContext.Request.Path.ToString().ToLower() == "/metrics" ||
                    httpContext.Request.Path.ToString().ToLower() == "/healthz"
                )
                {
                    return false;
                }

                if (httpContext.Request.Method == HttpMethods.Options)
                {
                    return false;
                }

                return true;
            };
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
        })
        .AddConsoleExporter());

var app = builder.Build();
app.MapHealthChecks("/healthz");
app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("[Startup] ApplicationLifetime - Stopping");
});

app.MapGroup("/v1/customer").MapCustomerApi().AllowAnonymous().WithTags("Public");

app.UseOpenTelemetryPrometheusScrapingEndpoint();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerContext>();
    dbContext.Database.Migrate();
}

app.Run();

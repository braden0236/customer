using customer.Database;
using customer.Endpoints;
using customer.Repositories;
using customer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<CustomerContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("CustomerDB"));
});

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();
app.MapHealthChecks("/healthz");

app.MapGroup("/v1/customer").MapCustomerApi().AllowAnonymous().WithTags("Public");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerContext>();
    dbContext.Database.Migrate();
}

app.Run();

using customer.Contracts.Requests;
using customer.Database;
using customer.Extensions;
using customer.Repositories;
using customer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CustomerContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("CustomerDB"));
});

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

app.MapGet("/customer/{id:guid}", async (Guid id, ICustomerService _service) =>
{
    var customer = await _service.GetAsync(id);
    if (customer == null)
    {
        return Results.NoContent();
    }

    return Results.Ok(customer.ToCustomerResponse());
});

app.MapPost("/customer", async (CreateCustomerRequest req, ICustomerService _service) =>
{
    var customer = req.ToCustomer();
    var result = await _service.CreateAsync(customer);
    if (!result)
    {
        return Results.BadRequest();
    }

    return Results.StatusCode(StatusCodes.Status201Created);
});

app.MapGet("/customer", async (ICustomerService _service) =>
{
    var customers = await _service.GetAllAsync();
    if (customers == null)
    {
        return null;
    }

    return customers.Select(c => c.ToCustomerResponse());
});

app.MapPut("/customer/{id:guid}", async (Guid id, UpdateCustomerRequest req, ICustomerService _service) =>
{
    var existingCustomer = await _service.GetAsync(id);
    if (existingCustomer is null) {
        return Results.NotFound();
    }

    var customer = req.ToCustomer(id);
    var updated = await _service.UpdateAsync(customer);
    if (!updated) {
        return Results.NoContent();
    }

    return Results.Ok();
});

app.MapDelete("/customer/{id:guid}", async (Guid id, ICustomerService _service) =>
{
    var deleted = await _service.DeleteAsync(id);
    if (!deleted)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerContext>();
    dbContext.Database.Migrate();
}

app.Run();

using customer.Contracts.Requests;
using customer.Extensions;
using customer.Services;
using Monitoring;

namespace customer.Endpoints;

public static class CustomerEndpoint
{
    public static RouteGroupBuilder MapCustomerApi(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, ICustomerService _service, MetricInstrumentation instrumentation) =>
        {
            var customer = await _service.GetAsync(id);
            instrumentation.QueryCustomerCounter.Add(1);
            if (customer == null)
            {
                return Results.NoContent();
            }

            return Results.Ok(customer.ToCustomerResponse());
        });

        group.MapGet("/", async (ICustomerService _service, MetricInstrumentation instrumentation) =>
        {
            var customers = await _service.GetAllAsync();
            instrumentation.QueryCustomerCounter.Add(1);
            if (customers == null)
            {
                return null;
            }

            return customers.Select(c => c.ToCustomerResponse());
        });

        group.MapPost("/", async (CreateCustomerRequest req, ICustomerService _service) =>
        {
            var customer = req.ToCustomer();
            var result = await _service.CreateAsync(customer);
            if (!result)
            {
                return Results.BadRequest();
            }

            return Results.StatusCode(StatusCodes.Status201Created);
        });
        group.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest req, ICustomerService _service) =>
        {
            var existingCustomer = await _service.GetAsync(id);
            if (existingCustomer is null)
            {
                return Results.NotFound();
            }

            var customer = req.ToCustomer(id);
            var updated = await _service.UpdateAsync(customer);
            if (!updated)
            {
                return Results.NoContent();
            }

            return Results.Ok();
        });
        group.MapDelete("/{id:guid}", async (Guid id, ICustomerService _service) =>
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });

        return group;
    }
}

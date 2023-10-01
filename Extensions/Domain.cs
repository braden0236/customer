using customer.Contracts.Requests;
using customer.Contracts.Responses;
using customer.Domain;

namespace customer.Extensions;

public static class DomainExtension
{
    public static Customer ToCustomer(this CreateCustomerRequest req)
    {
        return new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Username = req.Username,
            FullName = req.FullName,
            Email = req.Email,
            DateOfBirth = req.DateOfBirth
        };
    }
    public static Customer ToCustomer(this UpdateCustomerRequest req, Guid id)
    {
        return new Customer
        {
            Id = id.ToString(),
            Username = req.Username,
            FullName = req.FullName,
            Email = req.Email,
            DateOfBirth = req.DateOfBirth
        };
    }

    public static CustomerResponse ToCustomerResponse(this Customer customer)
    {
        return new CustomerResponse
        {
            Id = Guid.Parse(customer.Id),
            Username = customer.Username,
            FullName = customer.FullName,
            Email = customer.Email,
            DateOfBirth = customer.DateOfBirth
        };
    }
}

using customer.Domain;
using customer.Repositories;

namespace customer.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<bool> CreateAsync(Customer customer)
    {
        var existingUser = await _customerRepository.GetAsync(new Guid(customer.Id));
        if (existingUser != null)
        {
            return false;
        }

        return await _customerRepository.CreateAsync(customer);
    }

    public async Task<Customer?> GetAsync(Guid id)
    {
        var customer = await _customerRepository.GetAsync(id);

        return customer;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        return await _customerRepository.UpdateAsync(customer);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _customerRepository.DeleteAsync(id);
    }
}

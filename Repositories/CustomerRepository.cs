using customer.Database;
using customer.Domain;
using Microsoft.EntityFrameworkCore;

namespace customer.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerContext _context;

    public CustomerRepository(CustomerContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Customer?> GetAsync(Guid id)
    {
        var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id.ToString());

        return customer;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        var customers = await _context.Customers.AsNoTracking().ToListAsync();

        return customers;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        _context.Update(customer);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id.ToString());
        if (customer == null)
        {
            return true;
        }
        _context.Customers.Remove(customer);

        return await _context.SaveChangesAsync() > 0;
    }

}

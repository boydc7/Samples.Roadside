using Samples.Roadside.Models;

namespace Samples.Roadside.Contracts;

public interface ICustomerService
{
    Task<Customer> GetByIdAsync(string id);
    Task<IReadOnlyList<Customer>> GetManyAsync(int limit = 500);
    Task InsertAsync(Customer customer);
    Task UpdateAsync(Customer customer);
}

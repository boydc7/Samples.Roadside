using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Sql.Models;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Sql;

public class SqlCustomerService : ICustomerService
{
    private readonly IRoadsideSqlProvider _roadsideSqlProvider;
    private readonly ITransform<Customer, SqlCustomer> _transform;

    public SqlCustomerService(IRoadsideSqlProvider roadsideSqlProvider,
                              ITransform<Customer, SqlCustomer> transform)
    {
        _roadsideSqlProvider = roadsideSqlProvider;
        _transform = transform;
    }

    public async Task<Customer> GetByIdAsync(string id)
    {
        var sqlCustomer = await _roadsideSqlProvider.GetByIdAsync<SqlCustomer>("dbo.customers", id);

        var customer = _transform.Transform(sqlCustomer);

        return customer;
    }

    public async Task<IReadOnlyList<Customer>> GetManyAsync(int limit = 500)
    {
        var sqlCustomers = await _roadsideSqlProvider.QueryAsync<SqlCustomer>(@"
SELECT  TOP (@Limit) 
        c.*
FROM    dbo.customers c;
",
                                                                                new
                                                                                {
                                                                                    Limit = limit
                                                                                });


        var customers = sqlCustomers.Select(sa => _transform.Transform(sa))
                                    .AsListReadOnly();

        return customers;
    }
    
    public async Task InsertAsync(Customer customer)
    {
        var sqlCustomer = _transform.Transform(customer);
        
        await _roadsideSqlProvider.ExecuteAsync("INSERT dbo.customers (Id, Name) VALUES (@Id, @Name);",
                                                new
                                                {
                                                    sqlCustomer.Id,
                                                    sqlCustomer.Name
                                                });
    }

    public async Task UpdateAsync(Customer customer)
    {
        var sqlCustomer = _transform.Transform(customer);
        
        await _roadsideSqlProvider.ExecuteAsync("UPDATE dbo.customers SET Name = @Name WHERE Id = @Id;",
                                                new
                                                {
                                                    sqlCustomer.Id,
                                                    sqlCustomer.Name
                                                });
    }
}

using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Sql.Models;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Sql.Transforms;

public class SqlCustomerTransformer : ITransform<Customer, SqlCustomer>
{
    public SqlCustomer Transform(Customer source)
        => new()
           {
               Id = source.Id,
               Name = source.Name
           };

    public Customer Transform(SqlCustomer source)
        => new()
           {
               Id = source.Id,
               Name = source.Name
           };
}

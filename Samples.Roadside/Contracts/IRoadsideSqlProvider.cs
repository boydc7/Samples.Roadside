namespace Samples.Roadside.Contracts;

public interface IRoadsideSqlProvider
{
    Task<T> GetByIdAsync<T>(string tableName, string id);
    Task<int> ExecuteAsync(string sql, object param = null);
    Task<IEnumerable<T>>  QueryAsync<T>(string sql, object param = null);
}

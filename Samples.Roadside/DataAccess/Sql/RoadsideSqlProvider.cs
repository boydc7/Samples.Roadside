using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Samples.Roadside.Contracts;

namespace Samples.Roadside.DataAccess.Sql;

public class RoadsideSqlProvider : IRoadsideSqlProvider
{
    private readonly string _connectionString;

    public RoadsideSqlProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlData") ?? throw new ArgumentNullException(nameof(configuration),
                                                                                                            "SqlData ConnectionString must be specified.");
    }

    public async Task<T> GetByIdAsync<T>(string tableName, string id)
    {
        await using var sqlConnection = new SqlConnection(_connectionString);

        var sqlT = await sqlConnection.QueryFirstOrDefaultAsync<T>($"SELECT t.* FROM {tableName} t WHERE t.Id = @Id;",
                                                                   new
                                                                   {
                                                                       Id = id
                                                                   },
                                                                   commandType: CommandType.Text);

        return sqlT;
    }

    public async Task<int> ExecuteAsync(string sql, object param = null)
    {
        await using var sqlConnection = new SqlConnection(_connectionString);

        var result = await sqlConnection.ExecuteAsync(sql, param, commandType: CommandType.Text);

        return result;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
    {
        await using var sqlConnection = new SqlConnection(_connectionString);

        var results = await sqlConnection.QueryAsync<T>(sql, param, commandType: CommandType.Text);

        return results;
    }
}

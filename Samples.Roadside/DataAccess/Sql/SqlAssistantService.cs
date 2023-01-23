using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Sql.Models;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Sql;

public class SqlAssistantService : IAssistantService
{
    private readonly IRoadsideSqlProvider _roadsideSqlProvider;
    private readonly ITransform<Assistant, SqlAssistant> _transform;

    public SqlAssistantService(IRoadsideSqlProvider roadsideSqlProvider,
                               ITransform<Assistant, SqlAssistant> transform)
    {
        _roadsideSqlProvider = roadsideSqlProvider;
        _transform = transform;
    }

    public async Task<Assistant> GetByIdAsync(string id)
    {
        var sqlAssistant = await _roadsideSqlProvider.GetByIdAsync<SqlAssistant>("dbo.assistants", id);

        var assistant = _transform.Transform(sqlAssistant);

        return assistant;
    }

    public async Task<IReadOnlyList<Assistant>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var sqlAssistants = await _roadsideSqlProvider.QueryAsync<SqlAssistant>(@"
SELECT  a.*
FROM    dbo.assistants a
WHERE   a.Id IN @AssistantIds;
", 
                                                                                new
                                                                                {
                                                                                    AssistantIds = ids
                                                                                });

        var assistants = sqlAssistants.Select(sa => _transform.Transform(sa))
                                      .AsListReadOnly();

        return assistants;
    }
    
    public async Task<IReadOnlyList<Assistant>> GetManyAsync(int limit = 500)
    {
        var sqlAssistants = await _roadsideSqlProvider.QueryAsync<SqlAssistant>(@"
SELECT  TOP (@Limit) 
        a.*
FROM    dbo.assistants a;
",
                                                                                new
                                                                                {
                                                                                    Limit = limit
                                                                                });


        var assistants = sqlAssistants.Select(sa => _transform.Transform(sa))
                                      .AsListReadOnly();

        return assistants;
    }
    
    public async Task InsertAsync(Assistant assistant)
    {
        var sqlAssistant = _transform.Transform(assistant);
        
        await _roadsideSqlProvider.ExecuteAsync("INSERT dbo.assistants (Id, Name, BaseLatitude, BaseLongitude, IsAvailable) VALUES (@Id, @Name, @BaseLatitude, @BaseLongitude, @IsAvailable);",
                                                new
                                                {
                                                    sqlAssistant.Id,
                                                    sqlAssistant.Name,
                                                    sqlAssistant.BaseLatitude,
                                                    sqlAssistant.BaseLongitude,
                                                    sqlAssistant.IsAvailable
                                                });
    }

    public async Task UpdateAsync(Assistant assistant)
    {
        var sqlAssistant = _transform.Transform(assistant);
        
        await _roadsideSqlProvider.ExecuteAsync("UPDATE dbo.assistants SET Name = @Name, BaseLatitude = @BaseLatitude, BaseLongitude = @BaseLongitude, IsAvailable = @IsAvailable WHERE Id = @Id;",
                                                new
                                                {
                                                    sqlAssistant.Id,
                                                    sqlAssistant.Name,
                                                    sqlAssistant.BaseLatitude,
                                                    sqlAssistant.BaseLongitude,
                                                    sqlAssistant.IsAvailable
                                                });
    }
}

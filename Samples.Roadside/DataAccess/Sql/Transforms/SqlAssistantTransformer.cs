using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Sql.Models;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Sql.Transforms;

public class SqlAssistantTransformer : ITransform<Assistant, SqlAssistant>
{
    public SqlAssistant Transform(Assistant source)
        => new()
           {
               Id = source.Id,
               Name = source.Name,
               BaseLongitude = source.BaseLongitude,
               BaseLatitude = source.BaseLatitude,
               IsAvailable = source.IsAvailable
           };

    public Assistant Transform(SqlAssistant source)
        => new()
           {
               Id = source.Id,
               Name = source.Name,
               BaseLongitude = source.BaseLongitude,
               BaseLatitude = source.BaseLatitude,
               IsAvailable = source.IsAvailable
           };
}

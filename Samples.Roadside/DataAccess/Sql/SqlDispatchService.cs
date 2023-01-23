using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Sql.Models;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Sql;

public class SqlDispatchService : IDispatchService
{
    private readonly IRoadsideSqlProvider _roadsideSqlProvider;
    private readonly ITransform<Assistant, SqlAssistant> _transform;

    public SqlDispatchService(IRoadsideSqlProvider roadsideSqlProvider,
                              ITransform<Assistant, SqlAssistant> transform)
    {
        _roadsideSqlProvider = roadsideSqlProvider;
        _transform = transform;
    }

    public async Task<(bool Success, string DispatchId)> TryDispatchAssistantAsync(string assistantId, string toCustomerId)
    {
        var dispatchIdResults = await _roadsideSqlProvider.QueryAsync<Guid?>(@"
BEGIN TRY;
    BEGIN TRAN;

    UPDATE  dbo.assistants
    SET     IsAvailable = 0
    WHERE   Id = @AssistantId
            AND IsAvailable = 1;

    IF @@ROWCOUNT <= 0
    BEGIN;
        ROLLBACK TRAN;

        SELECT  CAST(NULL AS uniqueidentifier) AS DispatchId;

        RETURN;
    END;

    DECLARE @DispatchId uniqueidentifier = NEWID();

    INSERT  dbo.dispatches
            (Id, CustomerId, AssistantId)
    VALUES  (@DispatchId, @CustomerId, @AssistantId);

    SELECT  @DispatchId AS DispatchId;

    COMMIT TRAN;
END TRY
BEGIN CATCH
    ROLLBACK TRAN;

    THROW;
END CATCH;
",
                                                                             new
                                                                             {
                                                                                 AssistantId = assistantId,
                                                                                 CustomerId = toCustomerId
                                                                             });

        var dispatchId = dispatchIdResults?.FirstOrDefault();

        return dispatchId.HasValue
                   ? (true, dispatchId.Value.ToString())
                   : (false, default);
    }

    public async Task ReleaseAssistantAsync(string assistantId)
    {
        await _roadsideSqlProvider.ExecuteAsync(@"
UPDATE  dbo.assistants
SET     IsAvailable = 1
WHERE   Id = @AssistantId
        AND IsAvailable = 0;
",
                                                new
                                                {
                                                    AssistantId = assistantId
                                                });
    }
}

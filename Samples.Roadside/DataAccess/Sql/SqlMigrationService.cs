using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Contracts;

namespace Samples.Roadside.DataAccess.Sql;

public class SqlMigrationService : IMigrateData
{
    public async Task<bool> MigrateAsync(IServiceProvider serviceProvider)
    {
        var logFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var log = logFactory.CreateLogger("SqlMigrationService");

        log.LogInformation("Starting SQL data migration");

        var sqlClient = serviceProvider.GetRequiredService<IRoadsideSqlProvider>();

        var created = (await sqlClient.QueryAsync<int>(@"
IF OBJECT_ID('dbo.assistants') IS NULL
BEGIN;
    CREATE TABLE dbo.assistants
    (
    Id varchar(50) not null primary key, 
    Name varchar(250) not null,
    BaseLatitude decimal(18,15) null,
    BaseLongitude decimal(18,15) null,
    IsAvailable bit not null default 1
    );

    SELECT 1 AS Row;
END;
ELSE
BEGIN;
    SELECT 0 AS Row;
END;
")).FirstOrDefault();

        created += (await sqlClient.QueryAsync<int>(@"
IF OBJECT_ID('dbo.customers') IS NULL
BEGIN;
    CREATE TABLE dbo.customers
    (
    Id varchar(50) not null primary key, 
    Name varchar(250) not null
    );

    SELECT 1 AS Row;
END;
ELSE
BEGIN;
    SELECT 0 AS Row;
END;
")).FirstOrDefault();

        created += (await sqlClient.QueryAsync<int>(@"
IF OBJECT_ID('dbo.dispatches') IS NULL
BEGIN;
    CREATE TABLE dbo.dispatches
    (
    Id varchar(50) not null primary key, 
    CustomerId varchar(50) not null,
    AssistantId varchar(50) not null,
    );

    SELECT 1 AS Row;
END;
ELSE
BEGIN;
    SELECT 0 AS Row;
END;
")).FirstOrDefault();

        log.LogInformation("SQL data migration complete");

        return created > 0;
    }
}

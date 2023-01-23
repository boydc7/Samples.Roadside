using Microsoft.Extensions.DependencyInjection;
using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Sql.Models;
using Samples.Roadside.DataAccess.Sql.Transforms;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Sql;

public static class SqlExtensions
{
    private static bool _providerAdded;
    
    public static IServiceCollection AddSqlServerModelServices(this IServiceCollection serviceCollection)
        => serviceCollection.AddSqlRoadsideProvider()
                            .AddSingleton<ICustomerService, SqlCustomerService>()
                            .AddSingleton<IAssistantService, SqlAssistantService>()
                            .AddSingleton<ITransform<Customer, SqlCustomer>, SqlCustomerTransformer>()
                            .AddSingleton<ITransform<Assistant, SqlAssistant>, SqlAssistantTransformer>();

    public static IServiceCollection AddSqlServerDispatchServices(this IServiceCollection serviceCollection)
        => serviceCollection.AddSqlRoadsideProvider()
                            .AddSingleton<IDispatchService, SqlDispatchService>()
                            .AddSingleton<ITransform<Assistant, SqlAssistant>, SqlAssistantTransformer>();

    private static IServiceCollection AddSqlRoadsideProvider(this IServiceCollection serviceCollection)
    {
        if (_providerAdded)
        {
            return serviceCollection;
        }

        _providerAdded = true;

        serviceCollection.AddSingleton<IRoadsideSqlProvider, RoadsideSqlProvider>()
                         .AddSingleton<IMigrateData, SqlMigrationService>();

        return serviceCollection;
    }
}

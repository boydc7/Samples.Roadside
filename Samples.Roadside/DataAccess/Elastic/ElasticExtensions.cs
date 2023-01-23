using System.Runtime.CompilerServices;
using System.Text;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using Samples.Roadside.Contracts;

namespace Samples.Roadside.DataAccess.Elastic;

public static class ElasticExtensions
{
    private static bool _clientAdded;
    
    public static IServiceCollection AddElasticAssistantLocationTracking(this IServiceCollection serviceCollection)
        => serviceCollection.AddElasticClient()
                            .AddSingleton<ILocationService, ElasticLocationService>();

    public static IServiceCollection AddElasticAssistantFinderServices(this IServiceCollection serviceCollection)
        => serviceCollection.AddElasticClient()
                            .AddSingleton<IAssistantFinderService, ElasticAssistantFinderService>();

    public static IServiceCollection AddElasticDispatchUpdating(this IServiceCollection serviceCollection)
        => serviceCollection.AddElasticClient()
                            .AddSingleton<IDispatchUpdateService, ElasticDispatchUpdateService>();

    public static IServiceCollection AddElasticClient(this IServiceCollection serviceCollection)
    {
        if (_clientAdded)
        {
            return serviceCollection;
        }

        _clientAdded = true;
        
        serviceCollection.AddSingleton<IElasticClient>(c =>
                                                       {
                                                           var configuration = c.GetRequiredService<IConfiguration>();

                                                           var uri = configuration.GetConnectionString("Elasticsearch") ?? "http://localhost:1792";
                                                           
                                                           var pool = new SingleNodeConnectionPool(new Uri(uri));

                                                           var esConnectionSettings = new ConnectionSettings(pool).ConnectionLimit(300)
                                                                                                                  .MaximumRetries(4)
                                                                                                                  .DisableAutomaticProxyDetection()
                                                                                                                  .MaxRetryTimeout(TimeSpan.FromMinutes(3))
                                                                                                                  .RequestTimeout(TimeSpan.FromSeconds(15));

                                                           return new ElasticClient(esConnectionSettings);
                                                       })
                         .AddSingleton<IMigrateData, ElasticMigrationService>();

        return serviceCollection;
    }
    
    public static bool Successful(this IResponse response)
        => response is { IsValid: true, OriginalException: null, ServerError: null };

    public static Exception ToException(this IResponse response, [CallerMemberName] string methodName = null) => response?.OriginalException ?? new Exception(ToFailureString(response, methodName));

    private static string ToFailureString(IResponse response, string methodName) => string.Concat("Search Exception from method [", methodName, "]. IsValid [", response?.IsValid, "], ServerErrror [", response?.ServerError, "], ",
                                                                                                  "Reason [", response?.ServerError?.Error?.Reason, "], DebugInfo [", response?.DebugInformation?.Left(500), "], Request [",
                                                                                                  response?.ApiCall?.RequestBodyInBytes == null
                                                                                                      ? string.Empty
                                                                                                      : Encoding.UTF8.GetString(response?.ApiCall?.RequestBodyInBytes).Left(500), "]");
}

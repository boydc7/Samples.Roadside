using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Elastic.Models;

namespace Samples.Roadside.DataAccess.Elastic;

public class ElasticMigrationService : IMigrateData
{
    public async Task<bool> MigrateAsync(IServiceProvider serviceProvider)
    {
        var logFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var log = logFactory.CreateLogger("ElasticMigrationService");

        log.LogInformation("Starting Elastic data migration");

        var createdAssitantLocationIndex = await ConfigureEsIndexAsync<EsAssistantLocation>(serviceProvider, EsIndexes.AssistantLocationIndex);

        log.LogInformation("Elastic data migration complete");

        return createdAssitantLocationIndex;
    }
    
    private static async Task<bool> ConfigureEsIndexAsync<T>(IServiceProvider provider, string indexName)
        where T : class
    {
        var esClient = provider.GetRequiredService<IElasticClient>();

        var indexExists = await esClient.Indices.ExistsAsync(indexName);

        if (indexExists?.Exists ?? false)
        {
            return false;
        }

        var indexCreateResponse = await esClient.Indices.CreateAsync(indexName,
                                                                     cid => cid.Map<T>(d => d.AutoMap()
                                                                                             .Dynamic(false))
                                                                               .IncludeTypeName(false)
                                                                               .Settings(s => s.NumberOfShards(5)
                                                                                               .NumberOfReplicas(0)
                                                                                               .RefreshInterval(new Time(TimeSpan.FromSeconds(3)))
                                                                                               .UnassignedNodeLeftDelayedTimeout(new Time(TimeSpan.FromMinutes(13)))
                                                                                               .Analysis(ad => ad.Analyzers(az => az.Language("default", l => l.Language(Language.English)))
                                                                                                                 .Normalizers(nd => nd.Custom("samplekeyword", kw => kw.Filters("lowercase", "asciifolding"))))));

        if (!indexCreateResponse.Successful())
        {
            throw indexCreateResponse.ToException();
        }
        
        return true;
    }
}

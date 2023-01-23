using Microsoft.Extensions.Logging;
using Nest;
using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Elastic.Models;

namespace Samples.Roadside.DataAccess.Elastic;

public class ElasticDispatchUpdateService : IDispatchUpdateService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticDispatchUpdateService> _log;

    public ElasticDispatchUpdateService(IElasticClient elasticClient,
                                        ILogger<ElasticDispatchUpdateService> log)
    {
        _elasticClient = elasticClient;
        _log = log;
    }

    public async Task ProcessDispatchUpdateAsync(string assistantId, bool isDispatched)
    {
        _log.LogDebug("Updating dispatch state for assistant [{AssistantId}] - [dispatched:{IsDispatched}]", assistantId, isDispatched);

        await _elasticClient.UpdateAsync<EsAssistantLocation, object>(EsAssistantLocation.GetDocumentPath(assistantId),
                                                                      u => u.Index(EsIndexes.AssistantLocationIndex)
                                                                            .Doc(new
                                                                                 {
                                                                                     IsAvailable = !isDispatched
                                                                                 }));
    }
}

using Microsoft.Extensions.Logging;
using Nest;
using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Elastic.Models;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Elastic;

public class ElasticLocationService : ILocationService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticLocationService> _log;

    public ElasticLocationService(IElasticClient elasticClient,
                                  ILogger<ElasticLocationService> log)
    {
        _elasticClient = elasticClient;
        _log = log;
    }
    
    public async Task<Geolocation> GetAssistantLocationAsync(string id)
    {
        var getResponse = await _elasticClient.GetAsync(EsAssistantLocation.GetDocumentPath(id));

        return new Geolocation
               {
                   Latitude = getResponse.Source.Location.Latitude,
                   Longitude = getResponse.Source.Location.Longitude
               };
    }
    
    public async Task UpdateAssistantLocationAsync(string assistantId, Geolocation location)
    {
        _log.LogDebug("Updating location for assistant [{AssistantId}] to lat/long [{Latitude}/{Longitude}]", assistantId, location.Latitude, location.Longitude);
        
        await _elasticClient.UpdateAsync<EsAssistantLocation, object>(EsAssistantLocation.GetDocumentPath(assistantId),
                                                                      u => u.Index(EsIndexes.AssistantLocationIndex)
                                                                            .Doc(new
                                                                                 {
                                                                                     AssistantId = assistantId,
                                                                                     Location = new GeoLocation(location.Latitude, location.Longitude)
                                                                                 })
                                                                            .Upsert(new EsAssistantLocation
                                                                                    {
                                                                                        AssistantId = assistantId,
                                                                                        Location = new GeoLocation(location.Latitude, location.Longitude),
                                                                                        IsAvailable = false
                                                                                    }));
    }
}

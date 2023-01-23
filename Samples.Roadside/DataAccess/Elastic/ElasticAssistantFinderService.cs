using Microsoft.Extensions.Logging;
using Nest;
using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Elastic.Models;
using Samples.Roadside.Models;

namespace Samples.Roadside.DataAccess.Elastic;

public class ElasticAssistantFinderService : IAssistantFinderService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticAssistantFinderService> _log;

    public ElasticAssistantFinderService(IElasticClient elasticClient,
                                         ILogger<ElasticAssistantFinderService> log)
    {
        _elasticClient = elasticClient;
        _log = log;
    }

    public async Task<IReadOnlyList<AssistantAndLocation>> FindNearestAssistantIdsAsync(Geolocation location, int skip, int take)
    {
        _log.LogDebug("Querying assistants closest to lat/long [{Latitude}/{Longitude}] with skip/take [{Skip}/{Take}]", location.Latitude, location.Longitude, skip, take);

        var esGeoLocation = GeoLocation.TryCreate(location.Latitude, location.Longitude) ?? throw new ArgumentOutOfRangeException(nameof(location), "Invalid Geolocation latitude/longitude passed");

        IEnumerable<Func<QueryContainerDescriptor<EsAssistantLocation>, QueryContainer>> getFilters()
        {
            yield return f => f.Term(p => p.IsAvailable, true);

            // Seems like a reasonable thing to limit the available search to a 100 mile radius - typically would be in a config or similar, but
            yield return f => f.GeoDistance(g => g.Distance(100, DistanceUnit.Miles)
                                                  .DistanceType(GeoDistanceType.Plane)
                                                  .Location(esGeoLocation)
                                                  .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                                  .Field(d => d.Location));
        }

        var search = new SearchDescriptor<EsAssistantLocation>().Index(EsIndexes.AssistantLocationIndex)
                                                                .Query(q => q.Bool(b => b.Filter(getFilters())))
                                                                .From(skip)
                                                                .Size(take)
                                                                .Sort(s => s.GeoDistance(g => g.Field(d => d.Location)
                                                                                               .DistanceType(GeoDistanceType.Plane)
                                                                                               .IgnoreUnmapped()
                                                                                               .Unit(DistanceUnit.Miles)
                                                                                               .Points(esGeoLocation)
                                                                                               .Ascending())
                                                                            .Ascending(d => d.AssistantId));

        var response = await _elasticClient.SearchAsync<EsAssistantLocation>(search);

        return response.Hits
                       .Select(h => new AssistantAndLocation
                                    {
                                        Id = h.Source.AssistantId,
                                        Latitude = h.Source.Location.Latitude,
                                        Longitude = h.Source.Location.Longitude
                                    })
                       .AsListReadOnly();
    }
}

using Nest;

namespace Samples.Roadside.DataAccess.Elastic.Models;

[ElasticsearchType(IdProperty = nameof(AssistantId))]
public class EsAssistantLocation
{
    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 50, Normalizer = "samplekeyword")]
    public string AssistantId { get; set; }
    
    // Auto-mapped to a geo_point ES type
    public Nest.GeoLocation Location { get; set; }
    
    [Boolean]
    public bool IsAvailable { get; set; }
    
    public static DocumentPath<EsAssistantLocation> GetDocumentPath(string forId)
        => new DocumentPath<EsAssistantLocation>(new Id(forId)).Index(EsIndexes.AssistantLocationIndex);    
}

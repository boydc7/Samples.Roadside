using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Samples.Roadside.Models;

public class SampleApiResult<T>
{
    [JsonPropertyName("data")]
    public T Data { get; init; }

    [JsonIgnore]
    [IgnoreDataMember]
    public bool Success { get; init; }

    [JsonIgnore]
    [IgnoreDataMember]
    public IReadOnlyList<string> ErrorMessages { get; init; }
}

public class SampleApiResults<T>
    where T : class
{
    [JsonPropertyName("data")]
    public IReadOnlyList<T> Data { get; set; }

    [JsonPropertyName("resultCount")]
    public int ResultCount => Data?.Count ?? 0;
}

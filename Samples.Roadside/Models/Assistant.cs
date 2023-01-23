namespace Samples.Roadside.Models;

public class Assistant
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double BaseLatitude { get; set; }
    public double BaseLongitude { get; set; }
    public bool IsAvailable { get; set; }
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
}

public class AssistantAndLocation
{
    public string Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
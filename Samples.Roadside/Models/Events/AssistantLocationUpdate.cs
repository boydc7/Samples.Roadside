namespace Samples.Roadside.Models.Events;

public record AssistantLocationUpdate
{
    public string AssistantId { get; set; }
    public Geolocation Location { get; set; }
}

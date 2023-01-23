using Samples.Roadside.Models;

namespace Samples.Roadside.Contracts;

public interface ILocationService
{
    Task<Geolocation> GetAssistantLocationAsync(string id);
    Task UpdateAssistantLocationAsync(string assistantId, Geolocation location);
}

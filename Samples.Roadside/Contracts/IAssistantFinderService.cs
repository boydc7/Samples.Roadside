using Samples.Roadside.Models;

namespace Samples.Roadside.Contracts;

public interface IAssistantFinderService
{
    Task<IReadOnlyList<AssistantAndLocation>> FindNearestAssistantIdsAsync(Geolocation location, int skip, int take);
}

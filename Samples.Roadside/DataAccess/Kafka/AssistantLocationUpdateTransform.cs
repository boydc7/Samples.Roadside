using Samples.Roadside.Contracts;
using Samples.Roadside.Models;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.DataAccess.Kafka;

public class AssistantLocationUpdateTransform : ITransform<AssistantLocationUpdate, AssistantLocation>
{
    public AssistantLocation Transform(AssistantLocationUpdate source)
        => new()
           {
               AssistantId = source.AssistantId,
               Latitude = source.Location.Latitude,
               Longitude = source.Location.Longitude
           };

    public AssistantLocationUpdate Transform(AssistantLocation source)
        => new()
           {
               AssistantId = source.AssistantId,
               Location = new Geolocation
                          {
                              Latitude = source.Latitude,
                              Longitude = source.Longitude
                          }
           };
}

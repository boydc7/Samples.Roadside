using Samples.Roadside.Contracts;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.DataAccess.Kafka;

public class AssistantDispatchedTransform : ITransform<AssistantDispatched, AssistantDispatch>
{
    public AssistantDispatch Transform(AssistantDispatched source)
        => new()
           {
               AssistantId = source.AssistantId,
               CustomerId = source.CustomerId,
               Dispatched = source.Dispatched
           };

    public AssistantDispatched Transform(AssistantDispatch source)
        => new()
           {
               AssistantId = source.AssistantId,
               CustomerId = source.CustomerId,
               Dispatched = source.Dispatched
           };
}

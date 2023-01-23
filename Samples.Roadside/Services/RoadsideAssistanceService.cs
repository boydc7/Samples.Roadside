using Microsoft.IdentityModel.Tokens;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.Services;

public class RoadsideAssistanceService : IRoadsideAssistanceService
{
    private readonly ILocationService _locationService;
    private readonly IAssistantFinderService _assistantFinderService;
    private readonly IAssistantService _assistantService;
    private readonly IDispatchService _dispatchService;
    private readonly IDispatchEventProducer _eventProducer;

    public RoadsideAssistanceService(ILocationService locationService,
                                     IAssistantFinderService assistantFinderService,
                                     IAssistantService assistantService,
                                     IDispatchService dispatchService,
                                     IDispatchEventProducer eventProducer)
    {
        _locationService = locationService;
        _assistantFinderService = assistantFinderService;
        _assistantService = assistantService;
        _dispatchService = dispatchService;
        _eventProducer = eventProducer;
    }

    public async Task UpdateAssistantLocation(Assistant assistant, Geolocation assistantLocation)
        => await _locationService.UpdateAssistantLocationAsync(assistant.Id, assistantLocation);

    public async Task<IReadOnlyList<Assistant>> FindNearestAssistants(Geolocation geolocation, int limit)
    {
        var nearestAssistants = await _assistantFinderService.FindNearestAssistantIdsAsync(geolocation, 0, limit);

        var assistants = await _assistantService.GetByIdsAsync(nearestAssistants.Select(n => n.Id));

        var results = assistants.Select(a =>
                                        {
                                            var nearest = nearestAssistants.First(n => n.Id.Equals(a.Id, StringComparison.OrdinalIgnoreCase));

                                            a.CurrentLatitude = nearest.Latitude;
                                            a.CurrentLongitude = nearest.Longitude;

                                            return a;
                                        }).AsListReadOnly();
        
        // Could front-load the available check here, but given my assumption that the location datasource will remain near-real-time
        // updated with availability info and there's no reason to slow down every single search for them, the "is the provider actually
        // available check" is left for the reservation attempt, and if that fails due to a provider not being available (i.e. a race condition
        // or a delay in propogating availability info or similar), it will fail and push the user back to try to reserve a different provider,
        // which seems reasonable given that it shouldn't happen frequently
        return results;
    }

    public async Task<Optional<Assistant>> ReserveAssistant(Customer customer, Geolocation customerLocation)
    {
        const int limit = 5;

        var skip = 0;
        var dispatched = false;
        string dispatchId = null;
        string assistantId = null;

        try
        {
            do
            {
                var nearestAssistantIds = await _assistantFinderService.FindNearestAssistantIdsAsync(customerLocation, skip, limit);

                if (nearestAssistantIds.IsNullOrEmpty())
                {
                    break;
                }

                foreach (var nearAssistantId in nearestAssistantIds)
                {
                    (dispatched, dispatchId) = await _dispatchService.TryDispatchAssistantAsync(nearAssistantId.Id, customer.Id);

                    if (dispatched)
                    {
                        assistantId = nearAssistantId.Id;

                        break;
                    }
                }

                skip += limit;
            } while (!dispatched);

            if (dispatched)
            {
                // While there's a slight chance that the dispatch could succeed and this could fail or something crash in between, I'm assuming
                // for the moment the risk of this and minimal handling is fine, for a few reasons:
                //  1. If this fails but dispatch succeeded, when the dispatch is completed/released, the downstream consumers will be in the proper
                //      dispatch state again
                //  2. For the time period that they are not in the proper state, the assistant will not be double-booked anyhow, as the dispatch
                //      above is responsible for only dispathing those assistants that are in an available state within the dispatch store
                //  3. While you can easily argue that this code path shouldn't be aware of what downstream consumers may do with this dispatch event,
                //      for simplicity at the moment I'm taking this approach. Should we need to adjust that, there's a handful of ways we could account
                //      for it here, namely:
                //          a. Wrap the dispatch with a start/complete that performs a longer running transaction that is only committed when 
                //              the logic below successfully completed
                //          b. Implement offsetting/reversing transaction logic in the dispatch service that we compensate with if the below logic fails
                //          c. Implement a transactional outbox like pattern where the dispatch transaction stores an outbox message that is transactionally
                //              atomic with the dispatch itself, then separate the logic below into a service that monitors that output and continues with
                //              the below to essentially wind up with an at-least-once delivery consistency pattern

                var eventTask = _eventProducer.ProduceAsync(new AssistantDispatched
                                                            {
                                                                AssistantId = assistantId,
                                                                CustomerId = customer.Id,
                                                                DispatchId = dispatchId,
                                                                Dispatched = true
                                                            });

                var assistantTask = _assistantService.GetByIdAsync(assistantId);

                await Task.WhenAll(eventTask, assistantTask);

                return new Optional<Assistant>(assistantTask.Result);
            }

            return Optional<Assistant>.Empty();
        }
        catch
        {
            if (dispatched && !assistantId.IsNullOrEmpty())
            {
                await _dispatchService.ReleaseAssistantAsync(assistantId);
            }

            throw;
        }
    }

    public async Task ReleaseAssistant(Customer customer, Assistant assistant)
    {
        await _dispatchService.ReleaseAssistantAsync(assistant.Id);

        await _eventProducer.ProduceAsync(new AssistantDispatched
                                          {
                                              AssistantId = assistant.Id,
                                              CustomerId = customer.Id,
                                              Dispatched = false
                                          });
    }
}

# Sample Roadside Assistance API(s)
Simple consolidated set of APIs/services to mimic functionality needed to support a very naive and simple roadside assistance system allowing a vehicle/driver at a particular location to find and book/reserve nearby towtrucks or service providers that have locations nearby (or are driving a truck nearby). 

## [Clone repo](#clone-repo)

```bash
git clone https://github.com/boydc7/Samples.Roadside
cd Samples.Roadside
```

## [Docker Run](#docker-run)
To run the entire stack in Docker (inlcuding the API), simply docker compose up:

```bash
docker compose up
```

NOTE: To rebuild and force recreation of the various containers:
```bash
docker compose up --build --force-recreate --renew-anon-volumes
```
## [Docker Run Helper](#docker-run-helper)
A simple bash script is also included to help with running the stack in various states.

```bash
bash docker-up-local.sh
```

TO RESET LOCAL ENV (including a rebuild):
```bash
bash docker-up-local.sh reset
```

TO START THE API (in addition to the default of just starting dependencies):
```bash
bash docker-up-local.sh api
```

TO START THE API AND REBUILD/RESET LOCAL ENV
```bash
bash docker-up-local.sh reset api
```

TO START THE API AND REBUILD CODE ONLY (without resetting local dependencies):
```bash
bash docker-up-local.sh build api
```

## [Docker Stop](#docker-stop)

To stop the stack:
```bash
docker compose -f docker-compose.yml down
```

## [Swagger](#swagger)
A Swagger doc for the API is available at the root, simply open the following url to inspect:

http://localhost:8082


## [Postman](#postman)
A [Postman](https://www.postman.com/) collection is included in the root of the project (see the [postman_collection.json](/postman_collection.json) file in the root of the repo) for use with exercising the various API endpoints as you like manually or in an automated fashion.

# [NOTES](#notes)

Whenever the stack is run with a fresh docker setup, the first time the project runs it will create some demo sample data of 4 customers and 4 assistants. The assistants will have locations starting off in a sort of square around the DC area.

You can view the demo assistants and customers by hitting the /customers and /assistants endpoints (see [swagger](#swagger) for more info).

# [ASSUMPTIONS](#assumptions)
For the sake of time and simplicity, I've made the following assumptions and/or taken the following shortcuts:

1. While typically many of these components would be separate codebases, services, libraries, etc., everything is consolidated into a single service monolith for sanity sake.
2. Again for the sake of time and complexity, while this type of system would typically have to take into account things like roads (you know, for trucks to drive on and all) and likely use a variety of information about related things (i.e. traffic, closures, speed limits, etc.) in addition to actually routing trucks to a vehicle and paths to them and all that, I'm simply using a simple lat/lon/geo based implemenation and assuming a straight line path/distance.
3. In a few places in code I made some design decisions with certain assumptions and commented as such, those include:

   * RoadsideAssistanceService.F~~~~indNearestAssistants - where I choose to not verify in the transactional store that an assis~~~~tant is actually available:
   ```text
   // Could front-load the available check here, but given my assumption that the location datasource will remain near-real-time
   // updated with availability info and there's no reason to slow down every single search for them, the "is the provider actually
   // available check" is left for the reservation attempt, and if that fails due to a provider not being available (i.e. a race condition
   // or a delay in propagating availability info or similar), it will fail and push the user back to try to reserve a different provider,
   // which seems reasonable given that it shouldn't happen frequently
   ```
   
   * RoadsideAssistanceService.ReserveAssistant - where I choose to not interlock the dispatch of an event with the actual dispatch in the datastore:
   ```text
    // While there's a slight chance that the dispatch could succeed and this could fail or something crash in between, I'm assuming
    // for the moment the risk of this and minimal handling is fine, for a few reasons:
    //  1. If this fails but dispatch succeeded, when the dispatch is completed/released, the downstream consumers will be in the proper
    //      dispatch state again
    //  2. For the time period that they are not in the proper state, the assistant will not be double-booked anyhow, as the dispatch
    //      above is responsible for only dispatching those assistants that are in an available state within the dispatch store
    //  3. While you can easily argue that this code path shouldn't be aware of what downstream consumers may do with this dispatch event,
    //      for simplicity at the moment I'm taking this approach. Should we need to adjust that, there's a handful of ways we could account
    //      for it here, namely:
    //          a. Wrap the dispatch with a start/complete that performs a longer running transaction that is only committed when 
    //              the logic below successfully completed
    //          b. Implement offsetting/reversing transaction logic in the dispatch service that we compensate with if the below logic fails
    //          c. Implement a transactional outbox like pattern where the dispatch transaction stores an outbox message that is transactionally
    //              atomic with the dispatch itself, then separate the logic below into a service that monitors that output and continues with
    //              the below to essentially wind up with an at-least-once delivery consistency pattern
   ```

4. I didn't implement any instrumentation, much logging, or any other similar sorts of "production-ready" capabilities that would normally be included for what I assume are obvious reasons

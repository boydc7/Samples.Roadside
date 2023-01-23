using Samples.Roadside.Models;

namespace Samples.Roadside.Contracts;

public interface IRoadsideAssistanceService
{
    /**
    * This method is used to update the location of the roadside assistant service provider.
    *
    * @param assistant represents the roadside assistance provider
    * @param assistantLocation represents the location of the roadside assistant
    */
    Task UpdateAssistantLocation(Assistant assistant, Geolocation assistantLocation);

    /**
    * This method returns a collection of roadside assistants ordered by their distance
    * from the input geo location
    *
    * @param geolocation - geolocation from which to search for assistants
    * @param limit - the number of assistants to return
    * @return a sorted collection of assistants ordered ascending by distance from geoLocation
    */
    Task<IReadOnlyList<Assistant>> FindNearestAssistants(Geolocation geolocation, int limit);

    /**
    * Method to reserve an assistant for a person that is stranded on the roadside due to a disabled vehicle
    *
    * @param customer - Represents a customer
    * @param customerLocation - Location of the customer
    * @return The Assistant that is on their way to help
    */
    Task<Optional<Assistant>> ReserveAssistant(Customer customer, Geolocation customerLocation);

    /**
    * Method to release an assistant either after they have completed work or the customer no longer needs help.
    *
    * @param customer - Represents a customer
    * @param assistant - An assistant that was previously reserved by the customer
    */
    Task ReleaseAssistant(Customer customer, Assistant assistant);
}

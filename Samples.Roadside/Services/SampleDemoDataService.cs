using Samples.Roadside.Contracts;
using Samples.Roadside.Models;

namespace Samples.Roadside.Services;

public class SampleDemoDataService : IDemoDataService
{
    private readonly ICustomerService _customerService;
    private readonly IAssistantService _assistantService;
    private readonly ILocationService _locationService;
    private readonly IRoadsideAssistanceService _roadsideAssistanceService;

    public SampleDemoDataService(ICustomerService customerService, 
                                 IAssistantService assistantService,
                                 ILocationService locationService,
                                 IRoadsideAssistanceService roadsideAssistanceService)
    {
        _customerService = customerService;
        _assistantService = assistantService;
        _locationService = locationService;
        _roadsideAssistanceService = roadsideAssistanceService;
    }
    
    public async Task CreateDemoDataAsync()
    {
        var customers = new List<Customer>
                        {
                            new Customer
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Gecko"
                            },
                            new Customer
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Maxwell"
                            },
                            new Customer
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Caveman"
                            },
                            new Customer
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Camel"
                            }
                        };
        
        var assistants = new List<Assistant>
                        {
                            new Assistant
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Mater",
                                IsAvailable = true
                            },
                            new Assistant
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Smokey",
                                IsAvailable = true
                            },
                            new Assistant
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Bandit",
                                IsAvailable = true
                            },
                            new Assistant
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Breaker",
                                IsAvailable = true
                            }
                        };

        var customerTasks = customers.Select(c => _customerService.InsertAsync(c)).ToArray();
        var assistantTasks = assistants.Select(a => _assistantService.InsertAsync(a)).ToArray();

        await Task.WhenAll(customerTasks);
        await Task.WhenAll(assistantTasks);

        // Set starting location for 4 assistants in each corner of a sort of square around DC
        /*
        S 38.780000
        N 39.020000
        W 77.210000
        E 76.850000    
        */
        var locationTasks = new Task[4];

        // SE
        locationTasks[0] = _locationService.UpdateAssistantLocationAsync(assistants[0].Id, new Geolocation
                                                                                           {
                                                                                               Latitude = 38.780000,
                                                                                               Longitude = 76.850000
                                                                                           });
        // SW
        locationTasks[1] = _locationService.UpdateAssistantLocationAsync(assistants[1].Id, new Geolocation
                                                                                           {
                                                                                               Latitude = 38.780000,
                                                                                               Longitude = 77.210000
                                                                                           });
        // NE
        locationTasks[2] = _locationService.UpdateAssistantLocationAsync(assistants[2].Id, new Geolocation
                                                                                           {
                                                                                               Latitude = 39.020000,
                                                                                               Longitude = 76.850000
                                                                                           });
        // NW
        locationTasks[3] = _locationService.UpdateAssistantLocationAsync(assistants[3].Id, new Geolocation
                                                                                           {
                                                                                               Latitude = 39.020000,
                                                                                               Longitude = 77.210000
                                                                                           });

        // Release (make available) all assistants to start
        var releaseTasks = assistants.Select(a => _roadsideAssistanceService.ReleaseAssistant(customers[0],
                                                                                              assistants.First(s => s.Id.Equals(a.Id, StringComparison.OrdinalIgnoreCase))))
                                     .ToArray();
        
        await Task.WhenAll(locationTasks);
        await Task.WhenAll(releaseTasks);
    }
}

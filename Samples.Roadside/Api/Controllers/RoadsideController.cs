using Microsoft.AspNetCore.Mvc;
using Samples.Roadside.Api.Models;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models;

namespace Samples.Roadside.Api.Controllers;

public class RoadsideController : RoadsideControllerBase
{
    private readonly IRoadsideAssistanceService _roadsideAssistanceService;
    private readonly ICustomerService _customerService;
    private readonly IAssistantService _assistantService;
    private readonly ILocationService _locationService;

    public RoadsideController(IRoadsideAssistanceService roadsideAssistanceService,
                              ICustomerService customerService,
                              IAssistantService assistantService,
                              ILocationService locationService)
    {
        _roadsideAssistanceService = roadsideAssistanceService;
        _customerService = customerService;
        _assistantService = assistantService;
        _locationService = locationService;
    }

    [HttpGet("find")]
    public async Task<ActionResult<SampleApiResults<Assistant>>> Get([FromQuery] QueryRoadside query)
    {
        var assistants = await _roadsideAssistanceService.FindNearestAssistants(new Geolocation
                                                                                {
                                                                                    Latitude = query.Latitude,
                                                                                    Longitude = query.Longitude
                                                                                },
                                                                                query.Limit);

        var result = assistants.AsOkSampleApiResults();

        return result;
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<SampleApiResult<Assistant>>> PostReserve([FromBody] PostReserveAssistant request)
    {
        var customer = await _customerService.GetByIdAsync(request.CustomerId);

        var optAssistant = await _roadsideAssistanceService.ReserveAssistant(customer, new Geolocation
                                                                                       {
                                                                                           Latitude = request.Latitude,
                                                                                           Longitude = request.Longitude
                                                                                       });

        return optAssistant.IsPresent()
                   ? optAssistant.Get().AsOkSampleApiResult()
                   : NotFound();
    }

    [HttpPut("release")]
    public async Task<NoContentResult> PostRelease([FromBody] PutReleaseAssistant request)
    {
        var customerTask = _customerService.GetByIdAsync(request.CustomerId);
        var assistantTask = _assistantService.GetByIdAsync(request.AssistantId);

        await Task.WhenAll(customerTask, assistantTask);

        await _roadsideAssistanceService.ReleaseAssistant(customerTask.Result, assistantTask.Result);

        return NoContent();
    }
}

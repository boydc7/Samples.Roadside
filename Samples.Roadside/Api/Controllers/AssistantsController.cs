using Microsoft.AspNetCore.Mvc;
using Samples.Roadside.Api.Models;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models;

namespace Samples.Roadside.Api.Controllers;

public class AssistantsController : RoadsideControllerBase
{
    private readonly IRoadsideAssistanceService _roadsideAssistanceService;
    private readonly IAssistantService _assistantService;
    private readonly ILocationService _locationService;

    public AssistantsController(IRoadsideAssistanceService roadsideAssistanceService,
                                IAssistantService assistantService,
                                ILocationService locationService)
    {
        _roadsideAssistanceService = roadsideAssistanceService;
        _assistantService = assistantService;
        _locationService = locationService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<SampleApiResult<Assistant>>> Get(string id)
    {
        var assitantTask = _assistantService.GetByIdAsync(id);
        var locationTask = _locationService.GetAssistantLocationAsync(id);

        await Task.WhenAll(assitantTask, locationTask);

        var assistant = assitantTask.Result;
        assistant.CurrentLatitude = locationTask.Result.Latitude;
        assistant.CurrentLongitude = locationTask.Result.Longitude;

        return assistant.AsOkSampleApiResult();
    }
    
    [HttpGet]
    public async Task<ActionResult<SampleApiResults<Assistant>>> Get()
    {
        var assistants = await _assistantService.GetManyAsync(limit: 500);
        
        var result = assistants.AsOkSampleApiResults();

        return result;
    }
    
    [HttpPut("{id}/location")]
    public async Task<NoContentResult> PutLocation(string id, [FromBody] PutAssistantLocation request)
    {
        var assistant = await _assistantService.GetByIdAsync(id);

        await _roadsideAssistanceService.UpdateAssistantLocation(assistant, new Geolocation
                                                                            {
                                                                                Latitude = request.Latitude,
                                                                                Longitude = request.Longitude
                                                                            });

        return NoContent();
    }
}

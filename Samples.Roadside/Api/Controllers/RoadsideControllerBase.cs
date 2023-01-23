using Microsoft.AspNetCore.Mvc;

namespace Samples.Roadside.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "roadside")]
public abstract class RoadsideControllerBase : ControllerBase { }

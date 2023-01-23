using Microsoft.AspNetCore.Mvc;
using Samples.Roadside.Api.Models;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models;

namespace Samples.Roadside.Api.Controllers;

public class CustomersController : RoadsideControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<SampleApiResult<Customer>>> Get(string id)
    {
        var customer = await _customerService.GetByIdAsync(id);

        return customer.AsOkSampleApiResult();
    }
    
    [HttpGet]
    public async Task<ActionResult<SampleApiResults<Customer>>> Get()
    {
        var customers = await _customerService.GetManyAsync(limit: 500);
        
        var result = customers.AsOkSampleApiResults();

        return result;
    }
}

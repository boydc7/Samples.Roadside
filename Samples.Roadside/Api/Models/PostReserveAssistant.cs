using System.ComponentModel.DataAnnotations;

namespace Samples.Roadside.Api.Models;

public class PostReserveAssistant
{
    [Required]
    public string CustomerId { get; set; }

    [Required]
    [Range(-90,90)]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-180,180)]
    public double Longitude { get; set; }
}

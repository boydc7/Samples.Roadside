using System.ComponentModel.DataAnnotations;

namespace Samples.Roadside.Api.Models;

public class QueryRoadside
{
    [Required]
    [Range(-90,90)]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-180,180)]
    public double Longitude { get; set; }

    [Range(1, 500)]
    public int Limit { get; set; } = 50;
}

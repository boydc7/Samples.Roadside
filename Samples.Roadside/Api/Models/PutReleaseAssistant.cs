using System.ComponentModel.DataAnnotations;

namespace Samples.Roadside.Api.Models;

public class PutReleaseAssistant
{
    [Required]
    public string AssistantId { get; set; }

    [Required]
    public string CustomerId { get; set; }
}

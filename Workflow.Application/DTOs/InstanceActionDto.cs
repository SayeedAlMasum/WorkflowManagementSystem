using System;

namespace Workflow.Application.DTOs;

public class InstanceActionDto
{
  public int StepId { get; set; }
    public required string Action { get; set; } // "Approve", "Reject", "Complete", etc.
    public string? Comments { get; set; }
}

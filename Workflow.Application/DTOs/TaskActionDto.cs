using System;

namespace Workflow.Application.DTOs;

public class TaskActionDto
{
    public required string Action { get; set; } // "Complete", "Reject", "Reassign", etc.
    public string? Comments { get; set; }
    public string? ReassignToUserId { get; set; }
}

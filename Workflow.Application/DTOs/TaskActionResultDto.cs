using System;

namespace Workflow.Application.DTOs;

public class TaskActionResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public bool IsWorkflowCompleted { get; set; }
    public string? NextStepName { get; set; }
    public string? NextAssignedToUserId { get; set; }
}

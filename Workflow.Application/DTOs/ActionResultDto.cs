using System;

namespace Workflow.Application.DTOs;

public class ActionResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public string? NextStepName { get; set; }
    public string? AssignedToUserId { get; set; }
}

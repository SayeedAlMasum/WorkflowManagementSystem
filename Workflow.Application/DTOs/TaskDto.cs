using System;
using Workflow.Domain.Entities;

namespace Workflow.Application.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public int WorkflowInstanceId { get; set; }
    public required string WorkflowName { get; set; }
    public required string StepName { get; set; }
    public int Order { get; set; }
    public StepStatus Status { get; set; }
    public DateTime? StartedDate { get; set; }
    public required string AssignedToUserId { get; set; }
    public string? Comments { get; set; }
}

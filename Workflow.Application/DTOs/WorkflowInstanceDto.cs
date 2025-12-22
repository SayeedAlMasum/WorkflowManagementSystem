using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Workflow.Domain.Entities;

namespace Workflow.Application.DTOs;

public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    [Required]
    public string WorkflowName { get; set; }
    [Required]
    public string CurrentStepId { get; set; }
    [Required]
    public string CurrentStepName { get; set; }
    [Required]
    public string CreatedById { get; set; }
    public DateTime CreatedDate { get; set; }
    public InstanceStatus Status { get; set; }
    public List<WorkflowInstanceStepDto> Steps { get; set; } = new List<WorkflowInstanceStepDto>();
}

public class WorkflowInstanceStepDto
{
    public int Id { get; set; }
    public required string StepName { get; set; }
    public int Order { get; set; }
    public StepStatus Status { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletedByUserId { get; set; }
    public string? CompletedByUserName { get; set; }
    public string? Comments { get; set; }
}

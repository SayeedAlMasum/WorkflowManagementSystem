using System.ComponentModel.DataAnnotations;

namespace Workflow.Domain.Entities;

public class WorkflowInstanceStep
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int WorkflowInstanceId { get; set; }

    [Required]
    public int WorkflowStepId { get; set; }

    [Required]
  [MaxLength(200)]
    public required string StepName { get; set; }

    public int Order { get; set; }

    public StepStatus Status { get; set; } = StepStatus.Pending;

    public string? AssignedToUserId { get; set; }

    public DateTime? StartedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string? CompletedByUserId { get; set; }

    public string? Comments { get; set; }

    // Navigation properties
    [Required]
    public required WorkflowInstance WorkflowInstance { get; set; }

    [Required]
    public required WorkflowStep WorkflowStep { get; set; }

    public AppUser? AssignedToUser { get; set; }

public AppUser? CompletedByUser { get; set; }
}

public enum StepStatus
{
    Pending,
    InProgress,
    Completed,
    Skipped
}

using System.ComponentModel.DataAnnotations;

namespace Workflow.Domain.Entities
{
    public class WorkflowInstance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkflowId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string CurrentStepId { get; set; }

        [Required]
        public required string CreatedById { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public InstanceStatus Status { get; set; }

        // Navigation
        [Required]
        public required Workflow Workflow { get; set; }

        public ICollection<WorkflowInstanceStep> Steps { get; set; } = new List<WorkflowInstanceStep>();
    }

    public enum InstanceStatus
    {
        InProgress,
        Completed,
        Cancelled
    }
}

using System.ComponentModel.DataAnnotations;

namespace Workflow.Domain.Entities
{
    public class WorkflowInstanceHistory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int WorkflowInstanceId { get; set; }
        
        public int? WorkflowStepId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string Action { get; set; } // "Submitted", "Approved", "Rejected", "Completed"
      
        [Required]
        public required string ActionTakenById { get; set; }
        
        public string? Comment { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public required WorkflowInstance WorkflowInstance { get; set; }
        
        public WorkflowStep? WorkflowStep { get; set; }
    }
}
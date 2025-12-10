namespace Workflow.Domain.Entities
{
    public class WorkflowInstanceHistory
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public int? WorkflowStepId { get; set; }
        public string Action { get; set; } // "Submitted", "Approved", "Rejected", "Completed"
        public required string ActionTakenById { get; set; }
        public string? Comment { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public required WorkflowInstance WorkflowInstance { get; set; }
        public WorkflowStep? WorkflowStep { get; set; }
    }
}
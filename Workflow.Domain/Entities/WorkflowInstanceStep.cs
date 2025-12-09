using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Domain.Entities
{
    public class WorkflowInstanceStep
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public int StepId { get; set; }
        public string ActionTakenById { get; set; }
        public TaskAction Action { get; set; }
        public string? Comment { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    public enum TaskAction
    {
        Approved,
        Rejected,
        Completed,
        Cancelled
    }
}

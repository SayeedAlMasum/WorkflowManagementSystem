using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Domain.Entities
{

    public class WorkflowInstance
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string CurrentStepId { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public InstanceStatus Status { get; set; }
        public Workflow Workflow { get; set; }
        public ICollection<WorkflowInstanceStep> Steps { get; set; } = new List<WorkflowInstanceStep>();
    }
    public enum InstanceStatus
    {
        InProgress,
        Completed,
        Cancelled
    }
}

namespace Workflow.Domain.Entities
{
    public class WorkflowStep
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string StepName { get; set; }
        public int Order { get; set; }
        public string RoleRequired { get; set; }
        public Workflow Workflow { get; set; }
    }
}
namespace Workflow.Domain.Entities
{
    public class Workflow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    }
}
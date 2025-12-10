using System.ComponentModel.DataAnnotations;

namespace Workflow.Domain.Entities
{
    public class Workflow
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Description { get; set; }

        [Required]
        public required string CreatedById { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    }
}

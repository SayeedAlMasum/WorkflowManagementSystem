using System.ComponentModel.DataAnnotations;

namespace Workflow.Domain.Entities
{

    public class WorkflowStep
    {
        [Key]
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        [Required]
        [MaxLength(200)]
        public required string StepName { get; set; }
        public int Order { get; set; }
        [Required]
        [MaxLength(100)]
        public required string RoleRequired { get; set; }
        public required Workflow Workflow { get; set; }
    }
}
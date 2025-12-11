using System.ComponentModel.DataAnnotations;

namespace Workflow.Domain.Entities
{

    public class WorkflowStep
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int WorkflowId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public required string StepName { get; set; }
        
        public int Order { get; set; }
        
        [MaxLength(100)]
        public string? RoleRequired { get; set; } // Optional - step can be open to all
        
        [Required]
        public required Workflow Workflow { get; set; }
    }
}
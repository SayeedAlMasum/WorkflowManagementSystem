using System.Collections.Generic;

namespace Workflow.Application.DTOs
{
    public class WorkflowDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public List<WorkflowStepDto> Steps { get; set; } = new List<WorkflowStepDto>();
    }

    public class WorkflowStepDto
    {
        public int Id { get; set; }
        public required string StepName { get; set; }
    }
}
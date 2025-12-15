using System.Collections.Generic;

namespace Workflow.Application.DTOs
{

    public class CreateWorkflowDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string CreatedById { get; set; }  // ? Added this
        public List<CreateWorkflowStepDto> Steps { get; set; } = new List<CreateWorkflowStepDto>();
    }
}
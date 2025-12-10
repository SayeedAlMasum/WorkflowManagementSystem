using System.Collections.Generic;

namespace Workflow.Application.DTOs
{

    public class CreateWorkflowDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<CreateWorkflowStepDto> Steps { get; set; } = new List<CreateWorkflowStepDto>();
    }
}
using System;

namespace Workflow.Application.DTOs
{

    public class StartInstanceDto
    {
        public int WorkflowId { get; set; }
        public required string CreatedById { get; set; }
        public string? Comments { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Workflow.Application.DTOs;

public class WorkflowTemplateDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string CreatedById { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new List<WorkflowStepDto>();
}

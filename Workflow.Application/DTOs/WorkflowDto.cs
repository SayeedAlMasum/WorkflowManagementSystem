using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Application.DTOs
{
    public class WorkflowDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<WorkflowStepDto> Steps { get; set; }
    }
    public class WorkflowStepDto
    {
        public int Id { get; set; }
        public string StepName { get; set; }

    }
}

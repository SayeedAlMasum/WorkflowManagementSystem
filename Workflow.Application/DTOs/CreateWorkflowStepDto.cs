using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Application.DTOs
{

    public class CreateWorkflowStepDto
    {
        public string StepName { get; set; }
        public int Order { get; set; }
        public string RoleRequired { get; set; }
    }
}
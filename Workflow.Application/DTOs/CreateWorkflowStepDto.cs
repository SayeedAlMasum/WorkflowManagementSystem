using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Application.DTOs
{
    internal class CreateWorkflowStepDto
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string RoleRequired { get; set; }
    }
}

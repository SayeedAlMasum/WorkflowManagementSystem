using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow.Application.DTOs;

namespace Workflow.Application.Interfaces
{
    public interface IWorkflowHistoryService
    {
        Task<List<WorkflowHistoryDto>> GetHistoryAsync(int instanceid);
    }
}

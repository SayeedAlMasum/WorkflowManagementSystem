using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow.Application.DTOs;

namespace Workflow.Application.Interfaces
{


    public interface IWorkflowInstanceService
    {
        Task<int> StartInstanceAsync(StartInstanceDto dto); // returns instance id
        Task<WorkflowInstanceDto> GetInstanceAsync(int id);
        Task<ActionResultDto> ActOnInstanceAsync(int instanceId, InstanceActionDto action, string actingUserId);
        Task<List<WorkflowInstanceDto>> GetMyInstancesAsync(string userId);
    }
}
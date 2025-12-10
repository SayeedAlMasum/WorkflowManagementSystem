using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow.Application.DTOs;

namespace Workflow.Application.Interfaces
{

    public interface ITaskService
    {
        Task<List<TaskDto>> GetMyTasksAsync(string userId);
        Task<TaskActionResultDto> CompleteTaskAsync(int taskId, TaskActionDto action, string userId);
    }
}
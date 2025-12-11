using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow.Application.DTOs;
using Workflow.Domain.Entities;

namespace Workflow.Application.Interfaces;

public interface ITaskService
{
    Task<List<TaskDto>> GetMyTasksAsync(string userId, StepStatus? status = null, int page = 1, int pageSize = 20);
    Task<TaskActionResultDto> CompleteTaskAsync(int taskId, TaskActionDto action, string userId);
}
using Microsoft.EntityFrameworkCore;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Domain.Entities;
using Workflow.Infrastructure.Persistence;

namespace Workflow.Infrastructure.Services
{

    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;
        private readonly IWorkflowInstanceService _instanceService;

        public TaskService(AppDbContext db, IWorkflowInstanceService instanceService)
        {
            _db = db;
            _instanceService = instanceService;
        }

        public async Task<List<TaskDto>> GetMyTasksAsync(string userId, StepStatus? status = null, int page = 1, int pageSize = 20)
        {
            var query = _db.WorkflowInstanceSteps
           .Include(s => s.WorkflowInstance)
              .ThenInclude(i => i.Workflow)
       .Where(s => s.AssignedToUserId == userId);

            // Apply status filter if provided
            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);
            else
                query = query.Where(s => s.Status == StepStatus.InProgress); // Default to InProgress

            var tasks = await query
                .OrderBy(s => s.StartedDate)
             .Skip((page - 1) * pageSize)
        .Take(pageSize)
             .ToListAsync();

            var taskDtos = tasks.Select(s => new TaskDto
            {
                Id = s.Id,
                WorkflowInstanceId = s.WorkflowInstanceId,
                WorkflowName = s.WorkflowInstance.Workflow.Name,
                StepName = s.StepName,
                Order = s.Order,
                Status = s.Status,
                StartedDate = s.StartedDate,
                AssignedToUserId = s.AssignedToUserId ?? userId,
                Comments = s.Comments
            }).ToList();

            return taskDtos;
        }

        public async Task<TaskActionResultDto> CompleteTaskAsync(int taskId, TaskActionDto action, string userId)
        {
            var task = await _db.WorkflowInstanceSteps
        .Include(s => s.WorkflowInstance)
        .FirstOrDefaultAsync(s => s.Id == taskId);

            if (task == null)
                return new TaskActionResultDto
                {
                    Success = false,
                    Message = "Task not found"
                };

            if (task.AssignedToUserId != userId)
                return new TaskActionResultDto
                {
                    Success = false,
                    Message = "Task is not assigned to you"
                };

            if (task.Status != StepStatus.InProgress)
                return new TaskActionResultDto
                {
                    Success = false,
                    Message = "Task is not in progress"
                };

            // Use instance service to handle the action
            var instanceAction = new InstanceActionDto
            {
                Action = action.Action,
                Comments = action.Comments
            };

            var result = await _instanceService.ActOnInstanceAsync(
                    task.WorkflowInstanceId,
                    instanceAction,
          userId);

            return new TaskActionResultDto
            {
                Success = result.Success,
                Message = result.Message,
                IsWorkflowCompleted = task.WorkflowInstance.Status == InstanceStatus.Completed,
                NextStepName = result.NextStepName,
                NextAssignedToUserId = result.AssignedToUserId
            };
        }
    }
}
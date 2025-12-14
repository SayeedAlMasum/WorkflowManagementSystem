using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Domain.Entities;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
     _logger = logger;
    }

    /// <summary>
    /// Get my tasks with optional filtering and pagination
    /// </summary>
    /// <param name="status">Filter by status (Pending, InProgress, Completed, Skipped)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks(
        [FromQuery] StepStatus? status = null,
        [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20)
    {
      try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
          return Unauthorized(new { Message = "User not authenticated" });

     if (page < 1) page = 1;
      if (pageSize < 1 || pageSize > 100) pageSize = 20;

    var tasks = await _taskService.GetMyTasksAsync(userId, status, page, pageSize);
            
  return Ok(new
  {
  Page = page,
         PageSize = pageSize,
                Tasks = tasks,
              Count = tasks.Count
            });
      }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user");
            return StatusCode(500, new { Message = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
 /// Complete a task (approve/reject)
    /// </summary>
    [HttpPost("{taskId}/complete")]
    public async Task<IActionResult> CompleteTask(int taskId, [FromBody] TaskActionDto action)
    {
        try
 {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
  if (string.IsNullOrEmpty(userId))
 return Unauthorized(new { Message = "User not authenticated" });

          var result = await _taskService.CompleteTaskAsync(taskId, action, userId);
            
  if (result.Success)
                return Ok(result);
      
return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {TaskId}", taskId);
 return StatusCode(500, new { Message = "An error occurred while completing the task" });
}
    }
}

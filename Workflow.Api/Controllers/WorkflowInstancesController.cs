using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/workflow-instances")]
[Authorize]
public class WorkflowInstancesController : ControllerBase
{
    private readonly IWorkflowInstanceService _instanceService;
    private readonly IWorkflowHistoryService _historyService;
    private readonly ILogger<WorkflowInstancesController> _logger;

    public WorkflowInstancesController(
   IWorkflowInstanceService instanceService,
    IWorkflowHistoryService historyService,
        ILogger<WorkflowInstancesController> logger)
    {
    _instanceService = instanceService;
        _historyService = historyService;
        _logger = logger;
  }

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartInstanceDto dto)
    {
        try
        {
var instanceId = await _instanceService.StartInstanceAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = instanceId }, new { InstanceId = instanceId });
        }
        catch (ArgumentException ex)
        {
       return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow instance");
   return StatusCode(500, new { Message = "An error occurred while starting the workflow instance" });
        }
    }

 /// <summary>
    /// Get workflow instance details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
 var instance = await _instanceService.GetInstanceAsync(id);
   return Ok(instance);
     }
        catch (KeyNotFoundException ex)
        {
 return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow instance {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the workflow instance" });
        }
    }

    /// <summary>
    /// Perform an action on a workflow instance (approve/reject/complete)
/// </summary>
    [HttpPost("{id}/act")]
    public async Task<IActionResult> Act(int id, [FromBody] InstanceActionDto action)
    {
  try
        {
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
          return Unauthorized(new { Message = "User not authenticated" });

            var result = await _instanceService.ActOnInstanceAsync(id, action, userId);
            
        if (result.Success)
       return Ok(result);
            
 return BadRequest(result);
     }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Error acting on workflow instance {Id}", id);
      return StatusCode(500, new { Message = "An error occurred while processing the action" });
        }
    }

/// <summary>
    /// Approve a workflow instance step
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] string? comments = null)
    {
        try
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User not authenticated" });

     var result = await _instanceService.ApproveAsync(id, userId, comments);
            
 if (result.Success)
       return Ok(result);
    
            return BadRequest(result);
        }
        catch (Exception ex)
{
            _logger.LogError(ex, "Error approving workflow instance {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while approving" });
        }
    }

    /// <summary>
 /// Reject a workflow instance step
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] string? comments = null)
    {
        try
        {
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
     if (string.IsNullOrEmpty(userId))
        return Unauthorized(new { Message = "User not authenticated" });

            var result = await _instanceService.RejectAsync(id, userId, comments);
            
 if (result.Success)
 return Ok(result);
            
       return BadRequest(result);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error rejecting workflow instance {Id}", id);
  return StatusCode(500, new { Message = "An error occurred while rejecting" });
        }
    }

    /// <summary>
    /// Get my workflow instances
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> My()
    {
        try
     {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
  return Unauthorized(new { Message = "User not authenticated" });

  var instances = await _instanceService.GetMyInstancesAsync(userId);
         return Ok(instances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user instances");
         return StatusCode(500, new { Message = "An error occurred while retrieving instances" });
        }
    }

    /// <summary>
    /// Get workflow instance history
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        try
        {
      var history = await _historyService.GetHistoryAsync(id);
            return Ok(history);
      }
        catch (Exception ex)
        {
_logger.LogError(ex, "Error retrieving workflow history for instance {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving history" });
    }
    }
}

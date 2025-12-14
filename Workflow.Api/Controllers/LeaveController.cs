using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;
    private readonly IWorkflowInstanceService _instanceService;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(
        ILeaveService leaveService,
        IWorkflowInstanceService instanceService,
        ILogger<LeaveController> logger)
    {
        _leaveService = leaveService;
        _instanceService = instanceService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a leave request and start workflow
    /// </summary>
    [HttpPost("requests")]
    public async Task<IActionResult> SubmitLeaveRequest([FromBody] CreateLeaveRequestDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User not authenticated" });

            // Validate dates
            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { Message = "End date must be after start date" });

            // Submit leave request
            var leaveId = await _leaveService.SubmitAsync(dto);

            // Start workflow instance (assume Leave Approval workflow has ID = 2, or make it configurable)
            // You can add a setting for the default leave workflow ID
            try
            {
                var startDto = new StartInstanceDto
                {
                    WorkflowId = 2, // TODO: Make this configurable or lookup by name
                    CreatedById = userId,
                    Comments = $"Leave request: {dto.LeaveType} from {dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}"
                };

                var instanceId = await _instanceService.StartInstanceAsync(startDto);

                return CreatedAtAction(
                    nameof(GetLeaveRequest),
                    new { id = leaveId },
                    new { LeaveRequestId = leaveId, WorkflowInstanceId = instanceId });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Leave request created but workflow failed to start for leave {LeaveId}", leaveId);
                return Ok(new
                {
                    LeaveRequestId = leaveId,
                    Message = "Leave request created but workflow could not be started. Please contact administrator."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting leave request");
            return StatusCode(500, new { Message = "An error occurred while submitting the leave request" });
        }
    }

    /// <summary>
    /// Get leave request details
    /// </summary>
    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetLeaveRequest(int id)
    {
        try
        {
            var leave = await _leaveService.GetAsync(id);
            return Ok(leave);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave request {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the leave request" });
        }
    }

    /// <summary>
    /// Get my leave requests
    /// </summary>
    [HttpGet("requests/my")]
    public async Task<IActionResult> GetMyLeaveRequests()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User not authenticated" });

            var leaves = await _leaveService.GetMyLeaveRequestsAsync(userId);
            return Ok(leaves);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user leave requests");
            return StatusCode(500, new { Message = "An error occurred while retrieving leave requests" });
        }
    }

    /// <summary>
    /// Update a leave request
    /// </summary>
    [HttpPut("requests/{id}")]
    public async Task<IActionResult> UpdateLeaveRequest(int id, [FromBody] CreateLeaveRequestDto dto)
    {
        try
        {
            // Validate dates
            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { Message = "End date must be after start date" });

            await _leaveService.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave request {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while updating the leave request" });
        }
    }

    /// <summary>
    /// Delete a leave request
    /// </summary>
    [HttpDelete("requests/{id}")]
    public async Task<IActionResult> DeleteLeaveRequest(int id)
    {
        try
        {
            await _leaveService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting leave request {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while deleting the leave request" });
        }
    }
}

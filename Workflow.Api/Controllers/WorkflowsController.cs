using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(IWorkflowService workflowService, ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new workflow template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowDto dto)
    {
        try
        {
            var id = await _workflowService.CreateWorkflowAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return StatusCode(500, new { Message = "An error occurred while creating the workflow" });
        }
    }

    /// <summary>
    /// Get all workflow templates
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var workflows = await _workflowService.GetAllAsync();
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflows");
            return StatusCode(500, new { Message = "An error occurred while retrieving workflows" });
        }
    }

    /// <summary>
    /// Get workflow template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var workflow = await _workflowService.GetByIdAsync(id);
            return Ok(workflow);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the workflow" });
        }
    }

    /// <summary>
    /// Update a workflow template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateWorkflowDto dto)
    {
        try
        {
            await _workflowService.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while updating the workflow" });
        }
    }

    /// <summary>
    /// Delete a workflow template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _workflowService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while deleting the workflow" });
        }
    }
}



